using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Ignores all comments and white space in the input stream, and serializes it into Jack-language tokens.
/// The token types are specified according to the Jack grammar.
/// </summary>
namespace JackAnalyzer
{
	class JackTokenizer
	{
		private readonly StreamReader _streamReader;

		private string _currentToken;

		public enum TokenType
		{
			KEYWORD,
			SYMBOL,
			IDENTIFIER,
			INT_CONST,
			STRING_CONST
		}

		public enum KeyWord
		{
			CLASS,
			METHOD,
			FUNCTION,
			CONSTRUCTOR,
			INT,
			BOOLEAN,
			CHAR,
			VOID,
			VAR,
			STATIC,
			FIELD,
			LET,
			DO,
			IF,
			ELSE,
			WHILE,
			RETURN,
			TRUE,
			FALSE,
			NULL,
			THIS
		}

		public JackTokenizer(string jackFilePath)
		{
			_streamReader = new StreamReader(jackFilePath);
		}

		public bool HasMoreTokens()
		{
			// TODO
			return false;
		}

		/// <summary>
		/// Gets the next token from the input
		/// and makes it the current token.
		/// This method should be called only if `HasMoreTokens` is true.
		/// </summary>
		public void Advance()
		{

		}

		/// <summary>
		/// Returns the type of the current token, as an enum.
		/// </summary>
		public TokenType GetTokenType()
		{
			// TODO
			return TokenType.IDENTIFIER;
		}

		/// <summary>
		/// Returns the keyword which is the current token, as an enum.
		/// This method should be called only if
		/// `GetTokenType` is KEYWORD.
		/// </summary>
		/// <returns></returns>
		public KeyWord GetKeyWord()
		{
			_ = Enum.TryParse<KeyWord>(_currentToken, out KeyWord keyWord);
			return keyWord;
		}

		/// <summary>
		/// Returns the character which is the current token.
		/// Should be called only if `GetTokenType` is SYMBOL.
		/// </summary>
		public char GetSymbol()
		{
			// TODO
			return 'a';
		}

		/// <summary>
		/// Returns the identifier which is the current token.
		/// Should be called only if `GetTokenType` is IDENTIFIER.
		/// </summary>
		public string GetIdentifier()
		{
			// TODO
			return string.Empty;
		}

		/// <summary>
		/// Returns the integer value of the current token. Should be called only if `GetTokenType` is INT_CONST
		/// </summary>
		public int GetIntVal()
		{
			TokenType tokenType = GetTokenType();
			if (tokenType != TokenType.INT_CONST)
			{
				throw new Exception("GetIntVal should be called only if `GetTokenType` is INT_CONST!");
			}
			return int.Parse(_currentToken);
		}

		/// <summary>
		/// Returns the string value of the current token without the two enclosing double quotes. 
		/// Should be called only if `GetTokenType` is STRING_CONST
		/// </summary>
		public string GetStringVal()
		{
			TokenType tokenType = GetTokenType();
			if (tokenType != TokenType.STRING_CONST)
			{
				throw new Exception("GetStringVal should be called only if `GetTokenType` is STRING_CONST!");
			}
			return _currentToken;
		}
	}
}
