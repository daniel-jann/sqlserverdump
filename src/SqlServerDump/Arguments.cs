using System.Text.RegularExpressions;
using System.Collections.Generic;
 
namespace CommandLine.Utility
{
	/// <summary>
	/// Arguments class
    /// Source: http://www.codeproject.com/KB/recipes/command_line.aspx
	/// </summary>
 
	public class Arguments : Dictionary<string,string>
	{
        private LinkedList<string> _orphanValues;

		// Constructor
		public Arguments(string[] Args, bool spaceIsAValidSeparator = true, bool considerOnlyOrphanValuesAtEnd = true)
		{
            _orphanValues = new LinkedList<string>();
			Regex splitter = new Regex( @"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			Regex remover = new Regex( @"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
 
			string parameter = null;
			string[] parts;
 
			// Valid parameters forms:
			// {-,/,--}param{ ,=,:}((",')value(",'))
			// Examples: 
			// -param1 value1 --param2 /param3:"Test-:-work" /param4=happy -param5 '--=nice=--'
			foreach (string Txt in Args)
			{
				// Look for new parameters (-,/ or --) and a
				// possible enclosed value (=,:)
				parts = splitter.Split(Txt, 3);
 
				switch (parts.Length)
				{
					// Found a value (for the last parameter 
					// found (space separator))
					case 1:
                        if (parameter != null)
                        {
                            if (!base.ContainsKey(parameter))
                            {
                                parts[0] = remover.Replace(parts[0], "$1");
                                base.Add(parameter, parts[0]);
                                if (considerOnlyOrphanValuesAtEnd) { _orphanValues.Clear(); }
                            }
                            parameter = null;
                        }
                        else
                        { // Value without parameter name, add it to orphan values array
                            _orphanValues.AddLast(parts[0]); 
                        }
						break;
					// Found just a parameter
					case 2:
						// The last parameter is still waiting. 
						// With no value, set it to true.
						if (parameter != null)
						{
							if (!base.ContainsKey(parameter))
							{
                                base.Add(parameter, "true");
                            }
                            if (considerOnlyOrphanValuesAtEnd) { _orphanValues.Clear(); }
						}
                        parameter = parts[1];
                        if (!spaceIsAValidSeparator)
                        {
                            if (!base.ContainsKey(parameter))
                            {
                                base.Add(parameter, "true");
                            }
                            if (considerOnlyOrphanValuesAtEnd) { _orphanValues.Clear(); }
                            parameter = null;
                        }
						break;
 
					// Parameter with enclosed value
					case 3:
						// The last parameter is still waiting. 
						// With no value, set it to true.
						if (parameter != null)
						{
                            if (!base.ContainsKey(parameter))
                            {
                                base.Add(parameter, "true");
                                if (considerOnlyOrphanValuesAtEnd) { _orphanValues.Clear(); }
                            }
						}
 
						parameter = parts[1];
 
						// Remove possible enclosing characters (",')
						if (!base.ContainsKey(parameter))
						{
							parts[2] = remover.Replace(parts[2], "$1");
                            base.Add(parameter, parts[2]);
                            if (considerOnlyOrphanValuesAtEnd) { _orphanValues.Clear(); }
						}
						parameter = null;
						break;
				}
			}
			// In case a parameter is still waiting
			if (parameter != null)
			{
				if (!base.ContainsKey(parameter))
				{
                    base.Add(parameter, "true");
                    if (considerOnlyOrphanValuesAtEnd) { _orphanValues.Clear(); }
				}
			}
		}

        public LinkedList<string> OrphanValues
        {
            get { return _orphanValues; }
        }
	}
}