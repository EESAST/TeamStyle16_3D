#region

using System;

#endregion

[Serializable]
public class AnimationProgressionVariables
{
	public int m_letter_value;
	public int m_line_value;
	public int m_word_value;

	public AnimationProgressionVariables(int letter_val, int word_val, int line_val)
	{
		m_letter_value = letter_val;
		m_word_value = word_val;
		m_line_value = line_val;
	}

	public int GetValue(AnimatePerOptions animate_per)
	{
		switch (animate_per)
		{
			case AnimatePerOptions.LETTER:
				return m_letter_value;
			case AnimatePerOptions.WORD:
				return m_word_value;
			case AnimatePerOptions.LINE:
				return m_line_value;
		}

		return m_letter_value;
	}
}