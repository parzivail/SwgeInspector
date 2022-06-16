using System.Text;

namespace StringReader.NET;

public class StringDataReader
{
	private string _str;

	public StringDataReader(string str)
	{
		_str = str;
	}

	public void ReadChar(char expected)
	{
		var found = PeekChar();
		if (found != expected)
			throw new InvalidDataException($"Expected '{expected}', found '{found}'");
		TakeChar();
	}

	public string TakeUntil(char delimiter)
	{
		var sb = new StringBuilder();
		
		char c;
		do
		{
			c = TakeChar();
			sb.Append(c);
		} while (c != delimiter);

		return sb.ToString();
	}

	public string TakeString(int length)
	{
		var taken = PeekString(length);
		_str = _str[length..];
		return taken;
	}

	public char TakeChar()
	{
		var found = PeekChar() ?? throw new EndOfStreamException();
		_str = _str[1..];
		return found;
	}

	public string PeekString(int length)
	{
		if (_str.Length < length)
			throw new EndOfStreamException();
		return _str[..length];
	}

	public char? PeekChar()
	{
		if (_str.Length == 0)
			return null;
		return _str[0];
	}
}