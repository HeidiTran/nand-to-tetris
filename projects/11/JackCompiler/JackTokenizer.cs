﻿using System;
using System.Collections.Generic;
using System.IO;
using static JackCompiler.TokenType;

/// <summary>
/// Ignores all comments and white space in the input stream, and serializes it into Jack-language tokens.
/// The token types are specified according to the Jack grammar.
/// </summary>
namespace JackCompiler
{
	class JackTokenizer
	{
		private readonly StreamReader _streamReader;

		private string _currentToken;

		private static readonly HashSet<string> _symbols = new()
		{
			"{",
			"}",
			"(",
			")",
			"[",
			"]",
			".",
			",",
			";",
			"+",
			"-",
			"*",
			"/",
			"&",
			"|",
			"<",
			">",
			"=",
			"~"
		};
		private static readonly HashSet<string> _keywords = new()
		{
			"class",
			"method",
			"function",
			"constructor",
			"int",
			"boolean",
			"char",
			"void",
			"var",
			"static",
			"field",
			"let",
			"do",
			"if",
			"else",
			"while",
			"return",
			"true",
			"false",
			"null",
			"this"
		};

		private bool _isSlashButNotComment; // denotes a forward slash but not one for comment

		public JackTokenizer(string jackFilePath)
		{
			_streamReader = new StreamReader(jackFilePath);
		}

		public bool HasMoreTokens()
		{
			IgnoreWhiteSpaceAndNewLine();
			if (_streamReader.EndOfStream)
			{
				return false;
			}

			int nextToken = _streamReader.Peek();
			if (nextToken == '/')
			{
				_streamReader.Read();
				nextToken = _streamReader.Peek();

				if (nextToken == -1)
				{
					_isSlashButNotComment = true;
					return true;
				}

				if (nextToken == '/' || nextToken == '*')
				{
					IgnoreComment((char)nextToken);
					return HasMoreTokens();
				}

				_isSlashButNotComment = true;
				return true;
			}

			return !_streamReader.EndOfStream;
		}

		/// <summary>
		/// Gets the next token from the input
		/// and makes it the current token.
		/// </summary>
		public void Advance()
		{
			if (_isSlashButNotComment)
			{
				_currentToken = "/";
				_isSlashButNotComment = false;
				return;
			}

			char token = (char)_streamReader.Read();
			if (_symbols.Contains(token.ToString()))
			{
				_currentToken = token.ToString();
			}
			else if (token == '"')
			{
				_currentToken = ConsumeStringConstant(token);
			}
			else if (short.TryParse(token.ToString(), out _))
			{
				_currentToken = ConsumeAllDigits(token);
			}
			else
			{
				_currentToken = ConsumeKwOrIdentifier(token);
			}
		}

		/// <summary>
		/// Should only be called when HasMoreTokens() return true
		/// </summary>
		/// <returns>The next character</returns>
		public char LookOneCharAhead() => (char)_streamReader.Peek();

		private void IgnoreWhiteSpaceAndNewLine()
		{
			int nextToken = _streamReader.Peek();
			while (nextToken != -1 && IsIgnoredTok((char)nextToken))
			{
				_streamReader.Read();
				nextToken = _streamReader.Peek();
			}
		}

		private static bool IsIgnoredTok(char token) 
			=> token == ' ' || token == '\n' || token == '\t' || token == '\r';

		private void IgnoreComment(char typeOfComment)
		{
			if (typeOfComment == '/')
			{
				_streamReader.ReadLine();

			}
			else if (typeOfComment == '*')
			{
				_streamReader.Read(); // Skip the *
				char token = (char)_streamReader.Read();
				int nextToken = _streamReader.Peek();
				while (nextToken != -1 && (token != '*' || nextToken != '/'))
				{
					token = (char)_streamReader.Read();
					nextToken = _streamReader.Peek();
				}
				_streamReader.Read(); // Skip the /
			}
		}

		private string ConsumeAllDigits(char firstToken)
		{
			List<char> res = new() { firstToken };
			int nextToken = _streamReader.Peek();
			while (nextToken != -1 && 
				short.TryParse(((char)nextToken).ToString(), out _))
			{
				res.Add((char)_streamReader.Read());
				nextToken = _streamReader.Peek();
			}
			return string.Join("", res);
		}

		private string ConsumeStringConstant(char firstToken)
		{
			List<char> res = new() { firstToken };
			int nextToken = _streamReader.Peek();
			while (nextToken != -1 && nextToken != '"')
			{
				res.Add((char)_streamReader.Read());
				nextToken = _streamReader.Peek();
			}
			res.Add((char)_streamReader.Read());
			return string.Join("", res);
		}

		private string ConsumeKwOrIdentifier(char firstToken)
		{
			List<char> res = new() { firstToken };
			int nextToken = _streamReader.Peek();
			while (nextToken != -1 && !IsIgnoredTok((char)nextToken) 
				&& !_symbols.Contains(((char)nextToken).ToString()))
			{
				res.Add((char)_streamReader.Read());
				nextToken = _streamReader.Peek();
			}
			return string.Join("", res);
		}

		/// <summary>
		/// Returns the type of the current token, as an enum.
		/// </summary>
		public TokenType GetTokenType()
		{
			if (_symbols.Contains(_currentToken))
			{
				return SYMBOL;
			}
			else if (double.TryParse(_currentToken, out double num) && num >= 0 && num <= short.MaxValue)
			{
				return INT_CONST;
			}
			else if (_keywords.Contains(_currentToken))
			{
				return KEYWORD;
			}
			else if (_currentToken.StartsWith("\"") && _currentToken.EndsWith("\""))
			{
				// TODO: Check that the inner text is a unicode char + not including double quote + not including newline
				return STRING_CONST;
			}
			return IDENTIFIER;
		}

		/// <summary>
		/// Returns the keyword which is the current token, as an enum.
		/// This method should be called only if
		/// `GetTokenType` is KEYWORD.
		/// </summary>
		/// <returns></returns>
		public Keyword GetKeyWord()
		{
			if (GetTokenType() != KEYWORD)
			{
				throw new Exception("GetKeyWord should be called only if `GetTokenType` is KEYWORD!");
			}
			_ = Enum.TryParse(_currentToken.ToUpperInvariant(), out Keyword keyWord);
			return keyWord;
		}

		/// <summary>
		/// Returns the character which is the current token.
		/// Should be called only if `GetTokenType` is SYMBOL.
		/// </summary>
		public char GetSymbol()
		{
			if (GetTokenType() != SYMBOL)
			{
				throw new Exception("GetSymbol should be called only if `GetTokenType` is SYMBOL!");
			}
			return char.Parse(_currentToken);
		}

		/// <summary>
		/// Returns the identifier which is the current token.
		/// Should be called only if `GetTokenType` is IDENTIFIER.
		/// </summary>
		public string GetIdentifier()
		{
			if (GetTokenType() != IDENTIFIER)
			{
				throw new Exception("GetIdentifier should be called only if `GetTokenType` is IDENTIFIER!");
			}
			return _currentToken;
		}

		/// <summary>
		/// Returns the integer value of the current token. Should be called only if `GetTokenType` is INT_CONST
		/// </summary>
		public int GetIntVal()
		{
			if (GetTokenType() != INT_CONST)
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
			if (GetTokenType() != STRING_CONST)
			{
				throw new Exception("GetStringVal should be called only if `GetTokenType` is STRING_CONST!");
			}
			return _currentToken[1..^1];
		}

		public string GetCurrentToken() => _currentToken;

		public void Close() => _streamReader.Close();
	}
}
