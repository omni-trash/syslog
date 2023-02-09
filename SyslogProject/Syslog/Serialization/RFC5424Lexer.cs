
namespace Syslog.Serialization
{
    /// <summary>
    /// Lexer for RFC 5424 Spec based on ABNF in the document
    /// </summary>
    internal class RFC5424Lexer : LexerBase
    {
        List<RFC5424Token> tokens = new();

        /// <summary>
        /// Initializes a new instance
        /// </summary>
        /// <param name="input"></param>
        private RFC5424Lexer(string input)
            : base(input)
        {

        }

        /// <summary>
        /// Tokenize the payload
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public static List<RFC5424Token> Tokenize(string payload)
        {
            RFC5424Lexer lexer = new(payload);
            return lexer.Tokenize();
        }

        /// <summary>
        /// Tokenizes the input
        /// </summary>
        /// <returns></returns>
        List<RFC5424Token> Tokenize()
        {
            index  = 0;
            tokens = new();

            // starts here
            SYSLOGMSG();
            return tokens;
        }

        // Token Filter, we wont these tokens only (flat list)
        readonly List<RFC5424TokenType> tokenFilter = new()
        {
            RFC5424TokenType.PRIVAL,
            RFC5424TokenType.VERSION,
            RFC5424TokenType.TIMESTAMP,
            RFC5424TokenType.HOSTNAME,
            RFC5424TokenType.APPNAME,
            RFC5424TokenType.PROCID,
            RFC5424TokenType.MSGID,
            RFC5424TokenType.MSG,
            RFC5424TokenType.SDID,
            RFC5424TokenType.PARAMNAME,
            RFC5424TokenType.PARAMVALUE,
        };

        /// <summary>
        /// Adds a token
        /// </summary>
        /// <param name="content">token content</param>
        /// <param name="type">token type</param>
        void AddToken(string content, RFC5424TokenType type)
        {
            // - we dont build a tree (even it's ez possible)
            // - we need only some types for result
            // - the order is correct for parsing

            // it is also possible to remove the AddToken commands
            // we dont want, but for completness we keep them here as is.

            if (tokenFilter.Contains(type))
            {
                tokens.Add(RFC5424Token.Create(content, type));
            }
        }

        /// <summary>
        /// Adds a token from current input string
        /// </summary>
        /// <param name="start">input start</param>
        /// <param name="end">input end</param>
        /// <param name="type">token type</param>
        /// <remarks>
        /// AddToken(index[start..end], ...) is not available in net47
        /// </remarks>
        void AddToken(int start, int end, RFC5424TokenType type)
        {
            AddToken(StringUtil.Range(input, start, end), type);
        }

        /// <summary>
        /// HEADER SP STRUCTURED-DATA [SP MSG]
        /// </summary>
        void SYSLOGMSG()
        {
            int start = index;

            HEADER();
            SP();
            STRUCTUREDDATA();

            if (!IsEndOfInput())
            {
                SP();
                MSG();
            }

            ThrowIf(!IsEndOfInput());

            AddToken(start, index, RFC5424TokenType.SYSLOGMSG);
        }

        /// <summary>
        /// PRI VERSION SP TIMESTAMP SP HOSTNAME SP APP-NAME SP PROCID SP MSGID
        /// </summary>
        void HEADER()
        {
            int start = index;

            PRI();
            VERSION();
            SP();
            TIMESTAMP();
            SP();
            HOSTNAME();
            SP();
            APPNAME();
            SP();
            PROCID();
            SP();
            MSGID();

            AddToken(start, index, RFC5424TokenType.HEADER);
        }

        /// <summary>
        /// <![CDATA["<" PRIVAL ">"]]>
        /// </summary>
        void PRI()
        {
            int start = index;

            ThrowIf(Current() != '<');
            Next();

            PRIVAL();

            ThrowIf(Current() != '>');
            Next();

            AddToken(start, index, RFC5424TokenType.PRI);
        }

        /// <summary>
        /// 1*3DIGIT ; range 0 .. 191
        /// </summary>
        void PRIVAL()
        {
            int start = index;

            NextWhile(RFC5424.IS_DIGIT, _ => index - start < 3);
            ThrowIf(index - start == 0);

            AddToken(start, index, RFC5424TokenType.PRIVAL);
        }

        /// <summary>
        /// NONZERO-DIGIT 0*2DIGIT
        /// </summary>
        void VERSION()
        {
            int start = index;

            ThrowIf(!RFC5424.IS_NONZERODIGIT(Current()));
            NextWhile(RFC5424.IS_DIGIT, _ => index - start < 3);

            AddToken(start, index, RFC5424TokenType.VERSION);
        }

        /// <summary>
        /// %d32
        /// </summary>
        void SP()
        {
            ThrowIf(!RFC5424.IS_SP(Current()));
            Next();
        }

        /// <summary>
        /// NILVALUE / FULL-DATE "T" FULL-TIME
        /// </summary>
        void TIMESTAMP()
        {
            if (NILVALUE())
            {
                return;
            }

            int start = index;

            FULLDATE();

            ThrowIf(Current() != 'T');
            Next();

            FULLTIME();

            AddToken(start, index, RFC5424TokenType.TIMESTAMP);
        }

        /// <summary>
        /// DATE-FULLYEAR "-" DATE-MONTH "-" DATE-MDAY
        /// </summary>
        void FULLDATE()
        {
            int start = index;

            DATEFULLYEAR();

            ThrowIf(Current() != '-');
            Next();

            DATEMONTH();

            ThrowIf(Current() != '-');
            Next();

            DATEMDAY();

            AddToken(start, index, RFC5424TokenType.FULLDATE);
        }

        /// <summary>
        /// 4DIGIT
        /// </summary>
        void DATEFULLYEAR()
        {
            int start = index;

            NextWhile(RFC5424.IS_DIGIT, _ => index - start < 4);
            ThrowIf(index - start != 4);

            AddToken(start, index, RFC5424TokenType.DATEFULLYEAR);
        }

        /// <summary>
        /// 2DIGIT  ; 01-12
        /// </summary>
        void DATEMONTH()
        {
            int start = index;

            NextWhile(RFC5424.IS_DIGIT, _ => index - start < 2);
            ThrowIf(index - start != 2);

            AddToken(start, index, RFC5424TokenType.DATEMONTH);
        }

        /// <summary>
        /// 2DIGIT  ; 01-28, 01-29, 01-30, 01-31 based on month/year
        /// </summary>
        void DATEMDAY()
        {
            int start = index;

            NextWhile(RFC5424.IS_DIGIT, _ => index - start < 2);
            ThrowIf(index - start != 2);

            AddToken(start, index, RFC5424TokenType.DATEMDAY);
        }

        /// <summary>
        /// PARTIAL-TIME TIME-OFFSET
        /// </summary>
        void FULLTIME()
        {
            int start = index;

            PARTIALTIME();
            TIMEOFFSET();

            AddToken(start, index, RFC5424TokenType.FULLTIME);
        }

        /// <summary>
        /// TIME-HOUR ":" TIME-MINUTE ":" TIME-SECOND [TIME-SECFRAC]
        /// </summary>
        void PARTIALTIME()
        {
            int start = index;

            TIMEHOUR();

            ThrowIf(Current() != ':');
            Next();

            TIMEMINUTE();

            ThrowIf(Current() != ':');
            Next();

            TIMESECOND();

            if (Current() == '.')
            {
                TIMESECFRAC();
            }

            AddToken(start, index, RFC5424TokenType.PARTIALTIME);
        }

        /// <summary>
        /// 2DIGIT  ; 00-23
        /// </summary>
        void TIMEHOUR()
        {
            int start = index;

            NextWhile(RFC5424.IS_DIGIT, _ => index - start < 2);
            ThrowIf(index - start != 2);

            AddToken(start, index, RFC5424TokenType.TIMEHOUR);
        }

        /// <summary>
        /// 2DIGIT  ; 00-59
        /// </summary>
        void TIMEMINUTE()
        {
            int start = index;

            NextWhile(RFC5424.IS_DIGIT, _ => index - start < 2);
            ThrowIf(index - start != 2);

            AddToken(start, index, RFC5424TokenType.TIMEMINUTE);
        }

        /// <summary>
        /// 2DIGIT  ; 00-59
        /// </summary>
        void TIMESECOND()
        {
            int start = index;

            NextWhile(RFC5424.IS_DIGIT, _ => index - start < 2);
            ThrowIf(index - start != 2);

            AddToken(start, index, RFC5424TokenType.TIMESECOND);
        }

        /// <summary>
        /// "." 1*6DIGIT
        /// </summary>
        void TIMESECFRAC()
        {
            int start = index;

            ThrowIf(Current() != '.');
            Next();

            int digit = index;
            NextWhile(RFC5424.IS_DIGIT, _ => index - digit < 6);
            ThrowIf(index - digit == 0);

            AddToken(start, index, RFC5424TokenType.TIMESECFRAC);
        }

        /// <summary>
        /// "Z" / TIME-NUMOFFSET
        /// </summary>
        void TIMEOFFSET()
        {
            int start = index;

            if (Current() == 'Z')
            {
                Next();                
            }
            else
            {
                TIMENUMOFFSET();
            }

            AddToken(start, index, RFC5424TokenType.TIMEOFFSET);
        }

        /// <summary>
        /// ("+" / "-") TIME-HOUR ":" TIME-MINUTE
        /// </summary>
        void TIMENUMOFFSET()
        {
            int start = index;

            switch(Current())
            {
                case '+':
                case '-':
                    Next();
                    break;
                default:
                    ThrowIf(true);
                    return;
            }

            TIMEHOUR();

            ThrowIf(Current() != ':');
            Next();

            TIMEMINUTE();

            AddToken(start, index, RFC5424TokenType.TIMENUMOFFSET);
        }

        /// <summary>
        /// NILVALUE / 1*255PRINTUSASCII
        /// </summary>
        void HOSTNAME()
        {
            if (NILVALUE())
            {
                return;
            }

            int start = index;

            NextWhile(RFC5424.IS_PRINTUSASCII, _ => index - start < RFC5424.MAXLEN_HOSTNAME);
            ThrowIf(index - start == 0);

            AddToken(start, index, RFC5424TokenType.HOSTNAME);
        }

        /// <summary>
        /// NILVALUE / 1*48PRINTUSASCII
        /// </summary>
        void APPNAME()
        {
            if (NILVALUE())
            {
                return;
            }

            int start = index;

            NextWhile(RFC5424.IS_PRINTUSASCII, _ => index - start < RFC5424.MAXLEN_APPNAME);
            ThrowIf(index - start == 0);

            AddToken(start, index, RFC5424TokenType.APPNAME);
        }

        /// <summary>
        /// NILVALUE / 1*128PRINTUSASCII
        /// </summary>
        void PROCID()
        {
            if (NILVALUE())
            {
                return;
            }

            int start = index;

            NextWhile(RFC5424.IS_PRINTUSASCII, _ => index - start < RFC5424.MAXLEN_PROCID);
            ThrowIf(index - start == 0);

            AddToken(start, index, RFC5424TokenType.PROCID);
        }

        /// <summary>
        /// NILVALUE / 1*32PRINTUSASCII
        /// </summary>
        void MSGID()
        {
            if (NILVALUE())
            {
                return;
            }

            int start = index;

            NextWhile(RFC5424.IS_PRINTUSASCII, _ => index - start < RFC5424.MAXLEN_MSGID);
            ThrowIf(index - start == 0);

            AddToken(start, index, RFC5424TokenType.MSGID);
        }

        /// <summary>
        /// NILVALUE / 1*SD-ELEMENT
        /// </summary>
        void STRUCTUREDDATA()
        {
            if (NILVALUE())
            {
                return;
            }

            int start = index;

            SDELEMENT();

            while (Current() == '[')
            {
                SDELEMENT();
            }

            AddToken(start, index, RFC5424TokenType.STRUCTUREDDATA);
        }

        /// <summary>
        /// "[" SD-ID *(SP SD-PARAM) "]"
        /// </summary>
        void SDELEMENT()
        {
            int start = index;

            ThrowIf(Current() != '[');
            Next();

            SDID();

            while (!IsEndOfInput() && RFC5424.IS_SP(Current()))
            {
                SP();
                SDPARAM();
            }

            ThrowIf(Current() != ']');
            Next();

            AddToken(start, index, RFC5424TokenType.SDELEMENT);
        }

        /// <summary>
        /// SD-NAME
        /// </summary>
        void SDID()
        {
            int start = index;

            SDNAME();

            AddToken(start, index, RFC5424TokenType.SDID);
        }

        /// <summary>
        /// 1*32PRINTUSASCII ; except '=', SP, ']', %d34(")
        /// </summary>
        void SDNAME()
        {
            int start = index;

            NextWhile(RFC5424.IS_SD_NAME, _ => index - start < RFC5424.MAXLEN_SDNAME);
            ThrowIf(index - start == 0);

            AddToken(start, index, RFC5424TokenType.SDNAME);
        }

        /// <summary>
        /// PARAM-NAME "=" %d34 PARAM-VALUE %d34
        /// </summary>
        void SDPARAM()
        {
            int start = index;

            PARAMNAME();

            ThrowIf(Current() != '=');
            Next();

            ThrowIf(Current() != '"');
            Next();

            PARAMVALUE();

            ThrowIf(Current() != '"');
            Next();

            AddToken(start, index, RFC5424TokenType.SDPARAM);
        }

        /// <summary>
        /// SD-NAME
        /// </summary>
        void PARAMNAME()
        {
            int start = index;
            SDNAME();
            AddToken(start, index, RFC5424TokenType.PARAMNAME);
        }

        /// <summary>
        /// UTF-8-STRING ; characters '"', '\' and ; ']' MUST be escaped.
        /// </summary>
        void PARAMVALUE()
        {
            int start = index;
            bool esc = false;

            NextWhile(c =>
            {
                if (esc)
                {
                    esc = false;
                    return true;
                }

                if (c == '\\')
                {
                    esc = true;
                    return true;
                }

                if (c == '"')
                {
                    // end
                    return false;
                }

                return true;
            });

            AddToken(start, index, RFC5424TokenType.PARAMVALUE);
        }

        /// <summary>
        /// MSG-ANY / MSG-UTF8
        /// MSG-ANY         = *OCTET ; not starting with BOM
        /// MSG-UTF8        = BOM UTF-8-STRING
        /// BOM             = %xEF.BB.BF
        /// </summary>
        void MSG()
        {
            int start = index;
            NextWhile(_ => true);

            if (index - start > 0)
            {
                AddToken(start, index, RFC5424TokenType.MSG);
            }
        }

        /// <summary>
        /// "-"
        /// </summary>
        /// <returns></returns>
        bool NILVALUE()
        {
            if (Current() == RFC5424.NILVALUE_CHAR)
            {
                Next();
                AddToken(RFC5424.NILVALUE_STRING, RFC5424TokenType.NILVALUE);
                return true;
            }

            return false;
        }
    }
}
