#region

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace HighlightingSystem
{
	[RequireComponent(typeof(Camera))]
	public class HighlightingBase : MonoBehaviour
	{
		// Add reference when Highlighter component is enabled
		public static void AddHighlighter(Highlighter h)
		{
			if (highlighters == null)
				highlighters = new List<Highlighter>();

			highlighters.Add(h);
		}

		// 
		protected virtual void Awake()
		{
			ShaderPropertyID.Initialize();
			go = gameObject;

			refCam = GetComponent<Camera>();
			if (highlighters == null)
				highlighters = new List<Highlighter>();

			// Determine graphics device version
			var version = SystemInfo.graphicsDeviceVersion.ToLower();
			if (version.StartsWith("direct3d 11"))
				graphicsDeviceVersion = D3D11;
			else if (version.StartsWith("opengl"))
				graphicsDeviceVersion = OGL;
			else
				graphicsDeviceVersion = D3D9;
		}

		/// <summary>
		///     Blend rendered highlightingBuffer with src RenderTexture to dst RenderTexture.
		/// </summary>
		/// <param name="src">Source RenderTexture.</param>
		/// <param name="dst">Destination RenderTexture.</param>
		public void BlitHighlighting(RenderTexture src, RenderTexture dst)
		{
			// In case highlightingBuffer wasn't rendered - simply blit source to destination
			if (highlightingBuffer == null)
			{
				Graphics.Blit(src, dst, blitMaterial);
				return;
			}

			// Copy src to dst and then overlay highlightingBuffer to dst
			Graphics.SetRenderTarget(dst);
			blitMaterial.SetTexture(ShaderPropertyID._MainTex, src);
			compMaterial.SetTexture(ShaderPropertyID._MainTex, highlightingBuffer);
			DoubleBlit(blitMaterial, 0, compMaterial, 0, src.texelSize.y);

			// Cleanup
			RenderTexture.ReleaseTemporary(highlightingBuffer);
			highlightingBuffer = null;
		}

		// Allow only single instance of the HighlightingBase component on a GameObject
		public bool CheckInstance()
		{
			var highlightingBases = GetComponents<HighlightingBase>();
			if (highlightingBases.Length > 1 && highlightingBases[0] != this)
			{
				enabled = false;
				var className = GetType().ToString();
				Debug.LogWarning(string.Format("HighlightingSystem : Only single instance of HighlightingRenderer / HighlightingMobile components is allowed on a single Gameobject! {0} has been disabled on GameObject with name '{1}'.", className, name));
				return false;
			}
			return true;
		}

		// 
		protected bool CheckSupported()
		{
			// Image Effects supported?
			if (!SystemInfo.supportsImageEffects)
			{
				Debug.LogWarning("HighlightingSystem : Image effects is not supported on this platform!");
				return false;
			}

			// Render Textures supported?
			if (!SystemInfo.supportsRenderTextures)
			{
				Debug.LogWarning("HighlightingSystem : RenderTextures is not supported on this platform!");
				return false;
			}

			// Required Render Texture Format supported?
			if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32))
			{
				Debug.LogWarning("HighlightingSystem : RenderTextureFormat.ARGB32 is not supported on this platform!");
				return false;
			}

			// HighlightingOpaque shader supported?
			if (!Highlighter.opaqueShader.isSupported)
			{
				Debug.LogWarning("HighlightingSystem : HighlightingOpaque shader is not supported on this platform!");
				return false;
			}

			// HighlightingTransparent shader is not supported
			if (!Highlighter.transparentShader.isSupported)
			{
				Debug.LogWarning("HighlightingSystem : HighlightingTransparent shader is not supported on this platform!");
				return false;
			}

			// Disable if HighlightingBlit shader is not supported
			if (!blitShader.isSupported)
			{
				Debug.LogWarning("HighlightingSystem : HighlightingBlit shader is not supported on this platform!");
				return false;
			}

			// Disable if HighlightingBlur shader is not supported
			if (!blurShader.isSupported)
			{
				Debug.LogWarning("HighlightingSystem : HighlightingBlur shader is not supported on this platform!");
				return false;
			}

			// Disable if HighlightingCut shader is not supported
			if (!cutShader.isSupported)
			{
				Debug.LogWarning("HighlightingSystem : HighlightingCut shader is not supported on this platform!");
				return false;
			}

			// Disable if HighlightingComposite shader is not supported
			if (!compShader.isSupported)
			{
				Debug.LogWarning("HighlightingSystem : HighlightingComposite shader is not supported on this platform!");
				return false;
			}

			return true;
		}

		// 
		protected void DoubleBlit(Material mat1, int pass1, Material mat2, int pass2, float texelSize1 = 1f)
		{
			var y1 = 0f;
			var y2 = 1f;
			if (texelSize1 < 0f)
				// To avoid one texel vertical offset in D3D9 mode
				if (graphicsDeviceVersion == D3D9)
				{
					y1 = 1f - texelSize1;
					y2 = -texelSize1;
				}
				else
				{
					y1 = 1f;
					y2 = 0f;
				}
			var z = 0f;

			GL.PushMatrix();
			GL.LoadOrtho();

			mat1.SetPass(pass1);
			GL.Begin(GL.QUADS);
			// Unity uses a clockwise winding order for determining front-facing polygons. Important for stencil buffer!
			GL.TexCoord2(0f, y1);
			GL.Vertex3(0f, 0f, z); // Bottom-Left
			GL.TexCoord2(0f, y2);
			GL.Vertex3(0f, 1f, z); // Top-Left
			GL.TexCoord2(1f, y2);
			GL.Vertex3(1f, 1f, z); // Top-Right
			GL.TexCoord2(1f, y1);
			GL.Vertex3(1f, 0f, z); // Bottom-Right
			GL.End();

			mat2.SetPass(pass2);
			GL.Begin(GL.QUADS);
			GL.TexCoord2(0f, 0f);
			GL.Vertex3(0f, 0f, z);
			GL.TexCoord2(0f, 1f);
			GL.Vertex3(0f, 1f, z);
			GL.TexCoord2(1f, 1f);
			GL.Vertex3(1f, 1f, z);
			GL.TexCoord2(1f, 0f);
			GL.Vertex3(1f, 0f, z);
			GL.End();

			GL.PopMatrix();
		}

		// Performs one blur iteration
		protected void FourTapCone(RenderTexture src, RenderTexture dst, int iteration)
		{
			var off = blurMinSpread + iteration * blurSpread;
			blurMaterial.SetFloat(ShaderPropertyID._OffsetScale, off);
			Graphics.Blit(src, dst, blurMaterial);
			src.DiscardContents();
		}

		// 
		protected virtual void OnDestroy()
		{
			if (_blitMaterial != null)
				DestroyImmediate(_blitMaterial);
			if (_blurMaterial != null)
				DestroyImmediate(_blurMaterial);
			if (_cutMaterial != null)
				DestroyImmediate(_cutMaterial);
			if (_compMaterial != null)
				DestroyImmediate(_compMaterial);
		}

		// 
		protected virtual void OnDisable()
		{
			if (shaderCameraGO != null)
				DestroyImmediate(shaderCameraGO);

			if (highlightingBuffer != null)
			{
				RenderTexture.ReleaseTemporary(highlightingBuffer);
				highlightingBuffer = null;
			}
		}

		// 
		protected virtual void OnEnable()
		{
			if (!CheckInstance())
				return;

			isSupported = CheckSupported();
			if (isSupported)
				// Set initial intensity in blur shader
				blurMaterial.SetFloat(ShaderPropertyID._Intensity, _blurIntensity);
			else
			{
				enabled = false;
				Debug.LogWarning("HighlightingSystem : Highlighting System has been disabled due to unsupported Unity features on the current platform!");
			}
		}

		// Remove reference when Highlighter component is disabled
		public static void RemoveHighlighter(Highlighter h)
		{
			if (highlighters == null)
				return;

			var index = highlighters.IndexOf(h);
			if (index != -1)
				highlighters.RemoveAt(index);
		}

		/// <summary>
		///     Render highlighting to the highlightingBuffer using frameBuffer.depthBuffer.
		/// </summary>
		/// <param name="frameBuffer">Frame buffer RenderTexture, depthBuffer of which will be used to occlude highlighting.</param>
		public void RenderHighlighting(RenderTexture frameBuffer)
		{
			// Release highlightingBuffer if it wasn't released already
			if (highlightingBuffer != null)
			{
				RenderTexture.ReleaseTemporary(highlightingBuffer);
				highlightingBuffer = null;
			}

			if (!isSupported || !enabled || !go.activeInHierarchy)
				return;

			var aa = QualitySettings.antiAliasing;
			if (aa == 0)
				aa = 1;

			var depthAvailable = true;
			// Check if frameBuffer.depthBuffer is not available, contains garbage (when MSAA is enabled) or doesn't have stencil bits (when depth is 16 or 0)
			if (frameBuffer == null || frameBuffer.depth < 24)
				depthAvailable = false;

			// Reset aa value to 1 in case mainCam is in DeferredLighting Rendering Path
			if (refCam.actualRenderingPath == RenderingPath.DeferredLighting)
				aa = 1;
			// In case MSAA is enabled in forward/vertex lit rendeirng paths - depth buffer contains garbage
			else if (aa > 1)
				depthAvailable = false;

			// Check if framebuffer depth data availability has changed
			if (isDepthAvailable != depthAvailable)
			{
				isDepthAvailable = depthAvailable;
				// Update ZWrite value for all highlighting shaders correspondingly (isDepthAvailable ? ZWrite Off : ZWrite On)
				Highlighter.SetZWrite(isDepthAvailable ? 0f : 1f);
				if (isDepthAvailable)
					Debug.LogWarning("HighlightingSystem : Framebuffer depth data is available back again and will be used to occlude highlighting. Highlighting occluders disabled.");
				else
					Debug.LogWarning("HighlightingSystem : Framebuffer depth data is not available and can't be used to occlude highlighting. Highlighting occluders enabled.");
			}

			// Set global depth offset properties for highlighting shaders to the values which has this HighlightingBase component
			Highlighter.SetOffsetFactor(offsetFactor);
			Highlighter.SetOffsetUnits(offsetUnits);

			// Set this component as currently active HighlightingBase before enabling Highlighters
			current = this;

			// Turn on highlighting shaders on all highlighter components
			var count = 0;
			for (var i = 0; i < highlighters.Count; i++)
				if (highlighters[i].Highlight())
					count++;

			// Do nothing in case no Highlighters is currently visible
			if (count == 0)
			{
				current = null;
				return;
			}

			// If frameBuffer.depthBuffer is not available
			var w = Screen.width;
			var h = Screen.height;
			var depth = 24; // because stencil will be rendered to the highlightingBuffer.depthBuffer

			// If frameBuffer.depthBuffer is available
			if (isDepthAvailable)
			{
				w = frameBuffer.width;
				h = frameBuffer.height;
				depth = 0; // because stencil will be rendered to frameBuffer.depthBuffer
			}

			// Setup highlightingBuffer RenderTexture
			highlightingBuffer = RenderTexture.GetTemporary(w, h, depth, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, aa);
			if (!highlightingBuffer.IsCreated())
			{
				highlightingBuffer.filterMode = FilterMode.Point;
				highlightingBuffer.useMipMap = false;
				highlightingBuffer.wrapMode = TextureWrapMode.Clamp;
			}

			// Clear highlightingBuffer colorBuffer and clear depthBuffer only in case frameBuffer depth data is not available
			RenderTexture.active = highlightingBuffer;
			GL.Clear((isDepthAvailable ? false : true), true, Color.clear);

			// Use depth data from frameBuffer in case it is available. Use highlightingBuffer.depthBuffer otherwise
			var depthBuffer = isDepthAvailable ? frameBuffer.depthBuffer : highlightingBuffer.depthBuffer;

			if (!shaderCameraGO)
			{
				shaderCameraGO = new GameObject("HighlightingCamera");
				shaderCameraGO.hideFlags = HideFlags.HideAndDontSave;
				shaderCamera = shaderCameraGO.AddComponent<Camera>();
				shaderCamera.enabled = false;
			}

			shaderCamera.CopyFrom(refCam);
			//shaderCamera.projectionMatrix = mainCam.projectionMatrix;		// Uncomment this line if you have problems using Highlighting System with custom projection matrix on your camera
			shaderCamera.cullingMask = layerMask;
			shaderCamera.rect = new Rect(0f, 0f, 1f, 1f);
			shaderCamera.renderingPath = RenderingPath.Forward;
			shaderCamera.depthTextureMode = DepthTextureMode.None;
			shaderCamera.hdr = false;
			shaderCamera.useOcclusionCulling = false;
			shaderCamera.backgroundColor = new Color(0, 0, 0, 0);
			shaderCamera.clearFlags = CameraClearFlags.Nothing;
			shaderCamera.SetTargetBuffers(highlightingBuffer.colorBuffer, depthBuffer);

			// Get rid of "Tiled GPU Perf warning" if we're not in debug mode
#if !DEBUG_ENABLED
			frameBuffer.MarkRestoreExpected();
#endif
			shaderCamera.Render();

			// Extinguish all highlighters
			for (var i = 0; i < highlighters.Count; i++)
				highlighters[i].Extinguish();

			// Highlighting buffer rendering finished. Reset currently active HighlightingBase
			current = null;

			// Create two buffers for blurring the image
			var width = highlightingBuffer.width / _downsampleFactor;
			var height = highlightingBuffer.height / _downsampleFactor;
			var buffer = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
			var buffer2 = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 1);
			if (!buffer.IsCreated())
			{
				buffer.useMipMap = false;
				buffer.wrapMode = TextureWrapMode.Clamp;
			}
			if (!buffer2.IsCreated())
			{
				buffer2.useMipMap = false;
				buffer2.wrapMode = TextureWrapMode.Clamp;
			}

			// Copy highlighting buffer to the smaller texture
			Graphics.Blit(highlightingBuffer, buffer, blitMaterial);

			// Blur the small texture
			var oddEven = true;
			for (var i = 0; i < iterations; i++)
			{
				if (oddEven)
					FourTapCone(buffer, buffer2, i);
				else
					FourTapCone(buffer2, buffer, i);
				oddEven = !oddEven;
			}

			// Upscale blurred texture and cut stencil from it
			Graphics.SetRenderTarget(highlightingBuffer.colorBuffer, depthBuffer);
			cutMaterial.SetTexture(ShaderPropertyID._MainTex, oddEven ? buffer : buffer2);
			DoubleBlit(cutMaterial, 0, cutMaterial, 1);

			// Cleanup
			RenderTexture.ReleaseTemporary(buffer);
			RenderTexture.ReleaseTemporary(buffer2);
		}

		#region Static Fields

		// Contains reference to the currently rendering HighlightingBase component
		public static HighlightingBase current;

		// List with all enabled Highlighters in scene
		protected static List<Highlighter> highlighters;

		#endregion

		#region Inspector Fields

		// Depth offset factor for highlighting shaders
		public float offsetFactor = 0f;

		// Depth offset units for highlighting shaders
		public float offsetUnits = 0f;

		// Highlighting buffer size downsample factor
		public int _downsampleFactor = 4;

		// Blur iterations
		public int iterations = 2;

		// Blur minimal spread
		public float blurMinSpread = 0.65f;

		// Blur spread per iteration
		public float blurSpread = 0.25f;

		// Blurring intensity for the blur material
		public float _blurIntensity = 0.3f;

		// These properties available only in Editor - we don't need them in standalone build
#if UNITY_EDITOR
		// Downsampling factor getter/setter
		protected static readonly int[] _downsampleGet = { -1, 0, 1, -1, 2 };
		protected static readonly int[] _downsampleSet = { 1, 2, 4 };
		public int downsampleFactor { get { return _downsampleGet[_downsampleFactor]; } set { _downsampleFactor = _downsampleSet[value]; } }

		// Blur alpha intensity getter/setter
		public float blurIntensity
		{
			get { return _blurIntensity; }
			set
			{
				if (_blurIntensity != value)
				{
					_blurIntensity = value;
					if (Application.isPlaying)
						blurMaterial.SetFloat(ShaderPropertyID._Intensity, _blurIntensity);
				}
			}
		}
#endif

		#endregion

		#region Private Fields

		// Highlighting camera layers culling mask
		protected int layerMask = (1 << Highlighter.highlightingLayer);

		// Graphics device version identifiers
		private const int D3D9 = 0;
		private const int D3D11 = 1;
		private const int OGL = 2;

		// Current graphics device version: 0 = Direct3d 9 or unknown (default), 1 = Direct3D 11, 2 = OpenGL
		private int graphicsDeviceVersion = D3D9;

		// Camera for rendering highlighting buffer GameObject
		protected GameObject shaderCameraGO;

		// Camera for rendering highlighting buffer
		protected Camera shaderCamera;

		// RenderTexture with highlighting buffer
		protected RenderTexture highlightingBuffer;

		// This gameObject reference
		protected GameObject go;

		// Camera reference
		protected Camera refCam;

		// True if HighlightingSystem is supported on this platform
		protected bool isSupported;

		// True if framebuffer depth data is currently available (required for occlusion feature to work)
		protected bool _isDepthAvailable = true;
		public bool isDepthAvailable { get { return _isDepthAvailable; } protected set { _isDepthAvailable = value; } }

		// Simple Blit Shader
		protected static Shader _blitShader;

		protected static Shader blitShader
		{
			get
			{
				if (_blitShader == null)
					_blitShader = Shader.Find("Hidden/Highlighted/Blit");
				return _blitShader;
			}
		}

		// Blur Shader
		protected static Shader _blurShader;

		protected static Shader blurShader
		{
			get
			{
				if (_blurShader == null)
					_blurShader = Shader.Find("Hidden/Highlighted/Blur");
				return _blurShader;
			}
		}

		// Stencil Cut Shader
		protected static Shader _cutShader;

		protected static Shader cutShader
		{
			get
			{
				if (_cutShader == null)
					_cutShader = Shader.Find("Hidden/Highlighted/Cut");
				return _cutShader;
			}
		}

		// Compositing Shader
		protected static Shader _compShader;

		protected static Shader compShader
		{
			get
			{
				if (_compShader == null)
					_compShader = Shader.Find("Hidden/Highlighted/Composite");
				return _compShader;
			}
		}

		// Simple Blit Material
		protected static Material _blitMaterial;

		protected static Material blitMaterial
		{
			get
			{
				if (_blitMaterial == null)
				{
					_blitMaterial = new Material(blitShader);
					_blitMaterial.hideFlags = HideFlags.HideAndDontSave;
				}
				return _blitMaterial;
			}
		}

		// Blur Material
		protected static Material _blurMaterial;

		protected static Material blurMaterial
		{
			get
			{
				if (_blurMaterial == null)
				{
					_blurMaterial = new Material(blurShader);
					_blurMaterial.hideFlags = HideFlags.HideAndDontSave;
				}
				return _blurMaterial;
			}
		}

		// Stencil Cut Material
		protected static Material _cutMaterial;

		protected static Material cutMaterial
		{
			get
			{
				if (_cutMaterial == null)
				{
					_cutMaterial = new Material(cutShader);
					_cutMaterial.hideFlags = HideFlags.HideAndDontSave;
				}
				return _cutMaterial;
			}
		}

		// Compositing Material
		protected static Material _compMaterial;

		protected static Material compMaterial
		{
			get
			{
				if (_compMaterial == null)
				{
					_compMaterial = new Material(compShader);
					_compMaterial.hideFlags = HideFlags.HideAndDontSave;
				}
				return _compMaterial;
			}
		}

		#endregion
	}
}