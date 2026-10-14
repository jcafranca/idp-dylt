using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using DotNetCS;
using HandyControl.Tools.Extension;
using Microsoft.VisualBasic.Logging;
using Mysqlx.Crud;
using Mysqlx.Notice;
using Mysqlx.Session;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static DDCInteligence.DataTypes.DatabaseTypes;

namespace DDCInteligence.Models
{
    public class RubberBandModel
    {
        public readonly RegexServices regexServices = new RegexServices();
        public readonly LogServices logs = new LogServices();
        public readonly RegexInfo regex = new RegexInfo();
        public readonly List<string> garbageWords = new List<string>
        {
            "FOB: D", " FOB:","SPECIAL INSTRUCTIONS", "INSTRUCTIONS", "SPECIAL", "ORDER NUMBER", "COMMENTS",
            "ATTN", "CONTACT", "ATTENTION", "NAME", "ADDRESS", "CITY/STATE/ZIP", "PHONE", "EMAIL",
            "NAME:", "ADDRESS:", "CITY/STATE/ZIP:", "PHONE:", "EMAIL:","SID#:", "TO :", "ATTENTION TO :",
            "TO:", "TO", "ATTN:", "ATTN", "CONTACT:", "CID#","CID#:","amp;", "PICKUP APPT#:", "DELIVERY APPT#:"
        };
        private readonly List<StateInfo> states = new List<StateInfo>
            {
                new StateInfo { State = "ALABAMA", Code = "AL" },
                new StateInfo { State = "ALASKA", Code = "AK" },
                new StateInfo { State = "ARIZONA", Code = "AZ" },
                new StateInfo { State = "ARKANSAS", Code = "AR" },
                new StateInfo { State = "CALIFORNIA", Code = "CA" },
                new StateInfo { State = "COLORADO", Code = "CO" },
                new StateInfo { State = "CONNECTICUT", Code = "CT" },
                new StateInfo { State = "DELAWARE", Code = "DE" },
                new StateInfo { State = "FLORIDA", Code = "FL" },
                new StateInfo { State = "GEORGIA", Code = "GA" },
                new StateInfo { State = "HAWAII", Code = "HI" },
                new StateInfo { State = "IDAHO", Code = "ID" },
                new StateInfo { State = "ILLINOIS", Code = "IL" },
                new StateInfo { State = "INDIANA", Code = "IN" },
                new StateInfo { State = "IOWA", Code = "IA" },
                new StateInfo { State = "KANSAS", Code = "KS" },
                new StateInfo { State = "KENTUCKY", Code = "KY" },
                new StateInfo { State = "LOUISIANA", Code = "LA" },
                new StateInfo { State = "MAINE", Code = "ME" },
                new StateInfo { State = "MARYLAND", Code = "MD" },
                new StateInfo { State = "MASSACHUSETTS", Code = "MA" },
                new StateInfo { State = "MICHIGAN", Code = "MI" },
                new StateInfo { State = "MINNESOTA", Code = "MN" },
                new StateInfo { State = "MISSISSIPPI", Code = "MS" },
                new StateInfo { State = "MISSOURI", Code = "MO" },
                new StateInfo { State = "ST. CHARLES", Code = "MO" },
                new StateInfo { State = "MONTANA", Code = "MT" },
                new StateInfo { State = "NEBRASKA", Code = "NE" },
                new StateInfo { State = "NEVADA", Code = "NV" },
                new StateInfo { State = "NEW HAMPSHIRE", Code = "NH" },
                new StateInfo { State = "NEW JERSEY", Code = "NJ" },
                new StateInfo { State = "NEW MEXICO", Code = "NM" },
                new StateInfo { State = "NEW YORK", Code = "NY" },
                new StateInfo { State = "NORTH CAROLINA", Code = "NC" },
                new StateInfo { State = "NORTH DAKOTA", Code = "ND" },
                new StateInfo { State = "OHIO", Code = "OH" },
                new StateInfo { State = "OKLAHOMA", Code = "OK" },
                new StateInfo { State = "OREGON", Code = "OR" },
                new StateInfo { State = "PENNSYLVANIA", Code = "PA" },
                new StateInfo { State = "RHODE ISLAND", Code = "RI" },
                new StateInfo { State = "SOUTH CAROLINA", Code = "SC" },
                new StateInfo { State = "SOUTH DAKOTA", Code = "SD" },
                new StateInfo { State = "TENNESSEE", Code = "TN" },
                new StateInfo { State = "TEXAS", Code = "TX" },
                new StateInfo { State = "UTAH", Code = "UT" },
                new StateInfo { State = "VERMONT", Code = "VT" },
                new StateInfo { State = "VIRGINIA", Code = "VA" },
                new StateInfo { State = "WASHINGTON", Code = "WA" },
                new StateInfo { State = "WEST VIRGINIA", Code = "WV" },
                new StateInfo { State = "WISCONSIN", Code = "WI" },
                new StateInfo { State = "WYOMING", Code = "WY" },
                new StateInfo { State = "ALBERTA", Code = "AB" },
                new StateInfo { State = "BRITISH COLUMBIA", Code = "BC" },
                new StateInfo { State = "MANITOBA", Code = "MB" },
                new StateInfo { State = "NEW BRUNSWICK", Code = "NB" },
                new StateInfo { State = "NEWFOUNDLAND AND LABRADOR", Code = "NL" },
                new StateInfo { State = "NOVA SCOTIA", Code = "NS" },
                new StateInfo { State = "NORTHWEST TERRITORIES", Code = "NT" },
                new StateInfo { State = "NUNAVUT", Code = "NU" },
                new StateInfo { State = "ONTARIO", Code = "ON" },
                new StateInfo { State = "PRINCE EDWARD ISLAND", Code = "PE" },
                new StateInfo { State = "QUEBEC", Code = "QC" },
                new StateInfo { State = "SASKATCHEWAN", Code = "SK" },
                new StateInfo { State = "YUKON", Code = "YT" },
                new StateInfo { State = "BARCELONETA", Code = "PR" },
                new StateInfo { State = "MONTERREY NUEVO LEON", Code = "MX" },
                new StateInfo { State = "GUADALAJARA JALISCO", Code = "MX" },
                new StateInfo { State = "CALGARY", Code = "AS" },
                new StateInfo { State = "STAMFORD", Code = "CT" },
            };
        public void DisplayOCR(List<List<OCR.Word>> inputOCRData)
        {
            List<string> arrList = new List<string>();
            foreach (List<DotNetCS.OCR.Word> wList in inputOCRData)
            {
                string str = "";
                foreach (DotNetCS.OCR.Word w1 in wList)
                {
                    str += " " + w1.text;
                }
                arrList.Add(SpaceSpecialCharacters(str.Trim()).ToUpper());
            }

            string line = "";
            foreach (string str in arrList)
            {
                line += " " + str;
            }
        }
        public string ReturnAsSingle(List<List<OCR.Word>> highlightedWords)
        {
            var text = highlightedWords.Select(t => string.Join(" ", t.Select(u => u.text)));
            return this.RemoveDoubleSpace(string.Join("\n", text));
        }
        public AccountInfo ParseOneLineFormat(List<string> rawData)
        {
            AccountInfo Info = new AccountInfo();

            // Join all lines into a single string for parsing
            string temp_string = string.Join(" ", rawData);

            // Clean up the input
            string cleaned = temp_string.Trim();

            // Remove "BILL TO" prefix if present
            cleaned = this.RemoveSpecialCharacters(Regex.Replace(cleaned, @"^BILL\s+TO\s+", "", RegexOptions.IgnoreCase));
            cleaned = this.CleanGarbageWords(cleaned);

            // For input like: "LABCORP PO BOX 2230 BURLINGTON NC 27215"
            // We need to extract from right to left: ZIP, State, City, then everything else

            // Extract ZIP code (last 5 digits, optionally with -4 more digits)
            var zipMatch = Regex.Match(cleaned, @"\b(\d{5}(?:-\d{4})?)\s*$");
            if (zipMatch.Success)
            {
                Info.zip = zipMatch.Groups[1].Value;
                cleaned = cleaned.Substring(0, zipMatch.Index).Trim();
            }
            else
                return null;

            // Extract state (2 letters at the end after removing ZIP)
            string[] words = cleaned.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 0)
            {
                string lastWord = words[words.Length - 1];
                var temp_state = GetState(lastWord, true);
                if (!string.IsNullOrEmpty(temp_state))
                {
                    Info.state = lastWord;
                    // Remove the state from the array and rejoin
                    cleaned = string.Join(" ", words, 0, words.Length - 1);
                }
            }

            // Extract city (last word after removing state and ZIP)
            var cityMatch = Regex.Match(cleaned, @"([a-zA-Z]+)$");
            if (cityMatch.Success)
            {
                Info.city = cityMatch.Groups[1].Value;
                cleaned = cleaned.Substring(0, cityMatch.Index).Trim();
            }

            // Now split what's left into name and address
            string[] parts = cleaned.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 1)
            {
                // First part is the company name
                Info.name = parts[0];

                // Everything else is the address
                if (parts.Length > 1)
                {
                    string address = string.Join(" ", parts, 1, parts.Length - 1);
                    Info.addresses.Add(address);
                }
            }

            return Info;
        }
        public List<ReferenceInfo> GetReferences(IEnumerable<string> refs)
        {
            try
            {
                List<ReferenceInfo> ret = new List<ReferenceInfo>();
                List<string> words = new List<string>();
                refs.ToList().ForEach(_ref => {
                    if (!string.IsNullOrEmpty(_ref) && !words.Contains(_ref))
                    {
                        ret.Add(new ReferenceInfo { description = _ref });
                    }
                });

                return ret;
            }
            catch (Exception ex)
            {
                this.logs.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(GetReferences));
            }
            return null;
        }
        public List<InstructionInfo> GetInstruction(IEnumerable<string> instructions)
        {
            List<string> garbarge_words = new List<string> { "SPECIAL INSTRUCIONS", "INSTRUCTIONS", "SPECIAL", "ORDER NUMBER", "COMMENTS" };
            List<InstructionInfo> ret = new List<InstructionInfo>();
            //List<string> words = new List<string>();
            //try
            //{
            //    for (int i = 0; i < instructions.Count(); i++)
            //    {
            //        string str = instructions.ToList()[i].ToString().ToUpper();
            //        bool isFound = false;
            //        foreach (string line in words)
            //        {
            //            if (str.Contains(line))
            //            {
            //                isFound = true;
            //                break;
            //            }
            //        }
            //        if (!isFound && !string.IsNullOrEmpty(str))
            //        {
            //            ret.Add(new InstructionInfo { value = str });
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    logs.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(GetInstruction));
            //}
            return ret;
        }
        public CityStateZipInfo GetCityStateZip(string sInput)
        {
            CityStateZipInfo ret = new CityStateZipInfo();

            if (string.IsNullOrEmpty(sInput))
                return ret;

            // Remove commas for processing but keep track of original format
            string cleanInput = sInput.Replace(",", " ");

            // Clean input
            cleanInput = RemoveDoubleSpace(cleanInput.Trim());

            string[] ss = cleanInput.Split(' ');

            int stateIndex = -1;
            int zipIndex = -1;
            string foundZip = "";

            // First pass: Find ZIP code (5 digits or 5+4 format)
            for (int i = 0; i < ss.Length; i++)
            {
                string part = ss[i].Trim();

                // Check if this part is exactly a valid ZIP code
                if (Regex.IsMatch(part, @"^\d{5}(-\d{4})?$"))
                {
                    foundZip = part;
                    zipIndex = i;
                    break;
                }
                else
                {
                    // Try to find ZIP code inside a longer string, but NOT as part of a longer number
                    Match zipMatch = Regex.Match(part, @"(?<!\d)\d{5}(-\d{4})?(?!\d)");

                    if (zipMatch.Success)
                    {
                        foundZip = zipMatch.Value;  // Get the whole match
                        zipIndex = i;

                        // Remove the ZIP code from the string and clean it
                        ss[i] = part.Replace(foundZip, "").Trim();

                        break;
                    }
                }
            }

            // Second pass: Find state (2-letter code)
            for (int i = 0; i < ss.Length; i++)
            {
                if (i == zipIndex && string.IsNullOrEmpty(ss[i]))
                    continue; // Skip if this was the ZIP index and now empty

                // Check if it's a valid 2-letter state code
                string part = ss[i].ToUpper().Trim();
                string tmpState = part.Length == 2 ? GetState(part, true).ToUpper() : GetState(part).ToUpper();
                if (!string.IsNullOrEmpty(tmpState))
                {
                    ret.State = part;
                    stateIndex = i;
                    break;
                }
            }

            // Third pass: Find city(working backwards from state, but skip address components)
            int cityEndIndex = stateIndex != -1 ? stateIndex :
                      (zipIndex != -1 ? zipIndex : ss.Length);

            // Build city from parts, but exclude PO BOX and similar address components
            List<string> cityParts = new List<string>();

            for (int i = cityEndIndex - 1; i >= 0; i--)
            {
                string part = ss[i].Trim();
                if (!string.IsNullOrEmpty(part))
                {
                    // Check if this part looks like a city name (not address component)
                    if (IsLikelyCityName(part))
                    {
                        cityParts.Insert(0, part); // Insert at beginning to maintain order
                    }
                    else if (IsAddressComponent(part.ToUpper()))
                    {
                        // Stop when we hit address components like PO, BOX, numbers, etc.
                        break;
                    }
                    else if (!part.All(char.IsDigit)) // Skip pure numbers but include mixed
                    {
                        cityParts.Insert(0, part);
                    }
                }
            }

            // Join city parts
            if (cityParts.Count > 0)
            {
                ret.City = string.Join(" ", cityParts);
            }


            // Set ZIP code
            ret.Zip = foundZip;

            // Look for country code after ZIP or state
            int countryStartIndex = Math.Max(
                zipIndex != -1 ? zipIndex + 1 : 0,
                stateIndex != -1 ? stateIndex + 1 : 0
            );

            for (int i = countryStartIndex; i < ss.Length; i++)
            {
                string part = ss[i].ToUpper().Trim();

                // Check for country indicators
                if (part == "USA" || part == "US" ||
                    part == "UNITED" || part == "STATES")
                {
                    ret.CountryCode = "US";
                }
                else if (!string.IsNullOrEmpty(part) && string.IsNullOrEmpty(ret.CountryCode))
                {
                    // Try to get country code from the part
                    string cc = GetCountryCode(part);
                    if (!string.IsNullOrEmpty(cc))
                        ret.CountryCode = cc;
                }
            }

            // Clean up results
            ret.City = ret.City?.Trim();
            ret.State = ret.State?.Trim();
            ret.Zip = ret.Zip?.Trim();
            ret.CountryCode = ret.CountryCode?.Trim();

            // Validate ZIP format one more time
            if (!string.IsNullOrEmpty(ret.Zip))
            {
                ret.Zip = GetZipCode(ret.Zip);
            }

            // Set hasValue if we have meaningful data
            ret.hasValue = !string.IsNullOrEmpty(ret.City) ||
                           !string.IsNullOrEmpty(ret.State);

            // Reset if no valid zip found
            if (string.IsNullOrEmpty(ret.Zip) || ret.Zip == "0")
            {
                ret = new CityStateZipInfo();
            }

            return ret;
        }
        public bool IsEmail(string input)
        {
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(input, emailPattern);
        }
        public string CleanStringKeepSingleSpace(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string noSpecialChars = Regex.Replace(input, @"[^a-zA-Z0-9 ]", "");     // keep letters, numbers, and spaces
            string singleSpaced = Regex.Replace(noSpecialChars, @"\s{2,}", " ");    // collapse multiple spaces to one
            return singleSpaced.Trim();
        }

        // Helper method for BILL TO format address components
        public bool IsOneLineAddressComponent(string part)
        {
            string[] addressComponents = { "ST", "STREET", "AVE", "AVENUE", "RD", "ROAD",
                                 "BLVD", "BOULEVARD", "DR", "DRIVE", "LN", "LANE",
                                 "CT", "COURT", "PL", "PLACE", "WAY", "PKWY", "PARKWAY" };

            return Array.Exists(addressComponents, component =>
                part.Equals(component, StringComparison.OrdinalIgnoreCase));
        }
        public string CleanGarbageWords(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string cleaned = input;

            // Remove garbage words (case insensitive)
            foreach (string garbageWord in garbageWords)
            {
                // Remove the garbage word followed by colon or standalone
                cleaned = Regex.Replace(cleaned?.ToUpper(), $@"\b{Regex.Escape(garbageWord.ToUpper())}\s*:?\s*", " ", RegexOptions.IgnoreCase);
            }

            // Clean up multiple spaces and trim
            cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

            return cleaned;
        }
        public string ExtractAddressFromLine(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string line = input.Trim();

            // Handle pattern: "Attention 2253 STANTONBURG to : Nieves, Sammie ROAD"
            // Goal: Extract "2253 STANTONBURG ROAD"

            // Step 1: Remove "Attention" or "ATTN" from the beginning
            line = Regex.Replace(line, @"^\s*(ATTENTION|ATTN)\s+", "", RegexOptions.IgnoreCase);

            // Step 2: Look for the pattern with "to :" separator
            var match = Regex.Match(line, @"^(.+?)\s+to\s*:\s*(.+?)\s+([A-Z]+)$", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                string streetPart = match.Groups[1].Value.Trim();      // "2253 STANTONBURG"
                string contactPart = match.Groups[2].Value.Trim();     // "Nieves, Sammie"
                string streetType = match.Groups[3].Value.Trim();      // "ROAD"

                // Combine street number/name with street type, skip contact
                return $"{streetPart} {streetType}";
            }

            // Step 3: Fallback - remove contact info pattern and clean
            // Remove anything between "to :" and the last word (which is likely street type)
            line = Regex.Replace(line, @"\s+to\s*:\s*[^A-Z]*?(?=\s+[A-Z]+$)", "", RegexOptions.IgnoreCase);

            // Step 4: Clean remaining garbage words
            foreach (string garbageWord in garbageWords)
            {
                line = Regex.Replace(line, $@"\b{Regex.Escape(garbageWord)}\s*:?\s*", "", RegexOptions.IgnoreCase);
            }

            // Step 5: Clean up multiple spaces
            line = Regex.Replace(line, @"\s+", " ").Trim();

            return line;
        }
        public string ExtractContactFromAttentionLine(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string line = input;

            // Pattern to match: "Attention 2253 STANTONBURG to : Nieves, Sammie ROAD"
            // We want to extract: "Nieves, Sammie"

            var contactMatch = Regex.Match(line, @"to\s*:\s*([A-Za-z\s,]+?)\s+[A-Za-z]+$", RegexOptions.IgnoreCase);

            if (contactMatch.Success)
            {
                return contactMatch.Groups[1].Value.Trim();
            }

            // Fallback: standard garbage word cleaning
            return this.CleanGarbageWords(line);
        }
        public string CleanContactName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            // Remove common prefixes and suffixes
            name = name.Replace(":", "").Replace("-", "").Trim();

            // Remove phone numbers if any remain
            name = Regex.Replace(name, @"\b\d{3}[-.]?\d{3}[-.]?\d{4}\b", "").Trim();

            return name;
        }
        public bool IsValidContactName(string name)
        {
            if (string.IsNullOrEmpty(name) || name.Length < 3) return false;

            // Check if it contains mostly alphabetic characters
            return Regex.IsMatch(name, @"^[A-Za-z\s.,-]{3,50}$") &&
                   !name.All(char.IsDigit) &&
                   !IsLikelyCompanyName(name.ToUpper());
        }
        public bool IsValidAddress(string address)
        {
            if (string.IsNullOrEmpty(address) || address.Length < 5) return false;

            // Must contain at least a number and some letters
            return Regex.IsMatch(address, @"\d") && Regex.IsMatch(address, @"[A-Za-z]");
        }
        public bool IsLikelyCompanyName(string line)
        {
            string[] companyIndicators = { "CORP", "CORPORATION", "INC", "LLC", "LTD", "COMPANY", "CO.", "LABORATORY", "LAB" };
            return companyIndicators.Any(indicator => line.Contains(indicator));
        }
        public bool IsAddressPattern(string line)
        {
            line = line.Trim().ToUpper();

            // Match P.O. Box or PO BOX patterns
            bool isPOBox = Regex.IsMatch(line, @"\bP(?:\.|O)?\.?\s*BOX\s*\d+\b");

            // Match standard street address patterns
            bool isStreet = Regex.IsMatch(line, @"\b\d+\s+[A-Z\s]+\s+(ST|STREET|AVE|AVENUE|RD|ROAD|BLVD|BOULEVARD|DR|DRIVE|LN|LANE|CT|COURT)\b");

            // Match keywords suggesting address info
            bool hasKeywords = line.Contains("DELIVERY") || line.Contains("INSIDE");

            // Match generic "number + words" if other patterns fail
            bool isGeneric = Regex.IsMatch(line, @"\b\d+\s+[A-Z\s]+\b");

            return isPOBox || isStreet || hasKeywords || isGeneric;
        }
        private bool IsAddressComponent(string word)
        {
            if (string.IsNullOrEmpty(word))
                return false;

            word = word.ToUpper();

            // Address components to exclude from city
            string[] addressComponents = {
                "PO", "BOX", "P.O.", "POST", "OFFICE",
                "SUITE", "STE", "UNIT", "APT", "APARTMENT",
                "FLOOR", "FL", "BUILDING", "BLDG", "ROOM", "RM",
                "DELIVERY", "DOCK", "RECEIVING"
            };

            // Check if it's a pure number (likely part of PO Box or street number)
            if (word.All(char.IsDigit) && word.Length > 2)
                return true;

            return addressComponents.Contains(word);
        }
        private bool IsLikelyCityName(string word)
        {
            if (string.IsNullOrEmpty(word) || word.Length < 2)
                return false;

            word = word.ToUpper();

            // Common city name patterns - mostly alphabetic
            if (Regex.IsMatch(word, @"^[A-Z]+$") && word.Length >= 3)
            {
                // Exclude common address abbreviations
                string[] addressAbbreviations = { "ST", "AVE", "RD", "BLVD", "DR", "LN", "CT", "PL", "WAY" };
                if (!addressAbbreviations.Contains(word))
                {
                    return true;
                }
            }

            // Allow city names with common patterns like "ST. LOUIS"
            if (Regex.IsMatch(word, @"^[A-Z]+\.?$") && !word.All(char.IsDigit))
            {
                return true;
            }

            return false;
        }
        public bool IsAddressContinuation(string line, string previousAddress) // Helper method to determine if a line is an address continuation
        {
            string upperLine = line.ToUpper().Trim();

            // Single words that commonly continue addresses
            string[] addressContinuations = { "SHOP", "SUITE", "STE", "UNIT", "APT", "APARTMENT",
                                     "FLOOR", "FL", "BUILDING", "BLDG", "ROOM", "RM" };

            // If it's just a single word that's a known continuation
            if (addressContinuations.Contains(upperLine))
            {
                return true;
            }

            // If it starts with common continuation patterns
            if (upperLine.StartsWith("SUITE ") || upperLine.StartsWith("STE ") ||
                upperLine.StartsWith("UNIT ") || upperLine.StartsWith("APT ") ||
                upperLine.StartsWith("SHOP ") || upperLine.StartsWith("FLOOR ") ||
                upperLine.StartsWith("FL ") || upperLine.StartsWith("ROOM ") ||
                upperLine.StartsWith("RM "))
            {
                return true;
            }

            // If it's a short line (likely continuation) and doesn't look like a new address
            if (line.Length <= 15 && !Regex.IsMatch(line, @"^\d+\s+\w+"))
            {
                return true;
            }

            return false;
        }
        public bool isValidate_Using_Regex(string rgEX, string value)
        {
            Regex regex = new Regex(rgEX, RegexOptions.None);
            return regex.IsMatch(value);
        }
        public bool isContact(string str)
        {
            List<string> invalid_words = new List<string> { "FOB", "DOUBLE STACK" };
            return !invalid_words.Any(word => str.ToUpper().Contains(word));
        }
        public bool RegexValidation(string sInput, List<string> lstRegexPatterns)
        {
            foreach (string pattern in lstRegexPatterns)
            {
                Regex r = new Regex(pattern);
                if (r.IsMatch(sInput.Trim()))
                {
                    return true;
                }
            }
            return false;

        }
        public string GetPhone(string str)
        {
            string ret = "";
            try
            {
                if (str.StartsWith("PO ") || str.StartsWith("P0 "))
                    return "";

                foreach (string r in regex.regexPatterns.PHONE)
                {
                    Match match = null;
                    Regex regex = new Regex(r);
                    if (regex.IsMatch(str))
                    {
                        match = regex.Match(str);
                        ret = match.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                logs.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(GetPhone));
            }
            return ret;
        }
        public string GetState(string str, bool isCode = false)
        {
            string ret = "";
            try
            {
                if (str.ToUpper().Trim().Contains("WASHINGTON DISTRICT OF COLUMBIA"))
                {
                    str = str.ToUpper().Trim().Replace("WASHINGTON DISTRICT OF COLUMBIA", "DISTRICT OF COLUMBIA WASHINGTON");
                }
                string[] split_value = str.Split(' ');
                for (int i = 0; i < split_value.Count() - 1; i++)
                {
                    if (i == 0) continue;
                    var code = states.Where(state => state.Code == split_value[i]).FirstOrDefault();
                    if (code != null) return code.Code;
                }
                for (int i = 0; i < states.Count() - 1; i++)
                {
                    if (isCode)
                    {
                        if (str.ToUpper().Contains(states[i].Code.ToUpper()))
                        {
                            ret = states[i].Code.ToUpper();
                            str = str.Replace(states[i].State, ret);
                            return ret;
                        }
                    }
                    else
                    {
                        if (str.ToUpper().Contains(states[i].State.ToUpper()))
                        {
                            ret = states[i].Code.ToUpper();
                            str = str.Replace(states[i].State, ret);
                            return ret;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logs.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(GetState));
            }
            return ret;
        }
        public string GetCountryCode(string str)
        {
            List<CountryInfo> country_code = new List<CountryInfo>
            {
                new CountryInfo { Keyword = "UNITED STATES#UNITED#USA#US".Split('#').ToList(), Code = "US" },
                new CountryInfo { Keyword = "CANADA".Split('#').ToList(), Code = "CA" }
            };
            string ret = "";
            try
            {
                bool isOk = false;
                for (int j = 0; j < country_code.Count; j++)
                {
                    // GET COUNTRY CODE
                    for (int h = 0; h < country_code[j].Keyword.Count; h++)
                    {
                        if (str.ToUpper().Contains(country_code[j].Keyword[h].ToString().ToUpper()))
                        {
                            ret = country_code[j].Code;
                            isOk = true;
                        }
                    }
                    if (isOk) break;
                }
            }
            catch (Exception ex)
            {
                logs.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(GetCountryCode));
            }
            return ret.Trim();
        }
        public string GetZipCode(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            var ret = regex.regexPatterns.ZIPCODE.Select(pattern =>
            {
                Match match = null;
                Regex regex = new Regex(pattern);
                if (regex.IsMatch(str?.Trim()))
                {
                    match = regex.Match(str.Trim());
                    return match.ToString().Replace(" ", "");
                }
                return null;
            }).FirstOrDefault(result => result != null);

            if (!string.IsNullOrEmpty(ret)) return ret;
            return "";
        }
        public string SpaceSpecialCharacters(string input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
            {
                if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append(' ');
                }
            }
            return sb.ToString();
        }
        public bool IsNumeric(string input)
        {
            // Attempt to parse the string as a double
            if (double.TryParse(input, out _))
            {
                return true;
            }
            // If parsing as double fails, attempt to parse as an integer
            else if (int.TryParse(input, out _))
            {
                return true;
            }
            // If parsing as an integer also fails, the string is not numeric
            else
            {
                return false;
            }
        }
        public string RemoveSpecialCharacters(string input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
            {
                if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        public string RemoveDoubleSpace(string str)
        {
            string ret = str;
            try
            {
                while (ret.Contains("  "))
                {
                    ret = ret.Replace("  ", " ").Trim();
                }
            }
            catch (Exception ex)
            {
                logs.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(RemoveDoubleSpace));
            }
            return ret;
        }
        public System.Drawing.Rectangle GetWordRectangle(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return System.Drawing.Rectangle.Empty;

            var inputs = input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (inputs.Length == 0)
                return System.Drawing.Rectangle.Empty;

            try
            {
                XDocument _doc = XDocument.Load(ViewModelLocator.Instance.Process.OCRPath);
                var collected_rectangles = new List<System.Drawing.Rectangle>();

                // Get all elements from XML
                var lineElements = _doc.Descendants("Line").ToList();
                var wordElements = _doc.Descendants("Word").ToList();

                // Find lines that start with the first word of input
                var matchingLines = lineElements.Where(line =>
                    line.Element("Text") != null &&
                    line.Element("Text").Value.StartsWith(inputs[0], StringComparison.OrdinalIgnoreCase)
                ).ToList();

                for (var i = matchingLines.Count - 1; i >= 0; i--)
                {
                    var line = matchingLines[i];

                    // Get word indexes from this line
                    var wordIndexElements = line.Descendants("Word").ToList();

                    // Convert word index elements to actual word elements
                    var lineWords = new List<XElement>();
                    foreach (var wordIndexElement in wordIndexElements)
                    {
                        if (int.TryParse(wordIndexElement.Value, out int wordIndex))
                        {
                            var word = wordElements.FirstOrDefault(w =>
                                w.Element("Index") != null &&
                                RemoveSpecialCharacters(w.Element("Index").Value) == RemoveSpecialCharacters(wordIndex.ToString()));

                            if (word != null)
                                lineWords.Add(word);
                        }
                    }

                    // Check if this line contains our complete phrase
                    if (DoesLineContainPhrase(lineWords, inputs))
                    {
                        // Get rectangles for the matching words
                        var phraseRectangles = GetPhraseBoundingRectangles(lineWords, inputs);
                        collected_rectangles.AddRange(phraseRectangles);
                    }
                }

                // If no words found, return empty rectangle
                if (collected_rectangles.Count == 0)
                {
                    return System.Drawing.Rectangle.Empty;
                }

                // Calculate bounding rectangle that encompasses all found word rectangles
                int minX = collected_rectangles.Min(r => r.X);
                int minY = collected_rectangles.Min(r => r.Y);
                int maxX = collected_rectangles.Max(r => r.Right);
                int maxY = collected_rectangles.Max(r => r.Bottom);

                int width = maxX - minX;
                int height = maxY - minY;

                // Simulating VB logic
                int num2 = /* your value here */ 0;  // replace this with actual logic or value
                width = (num2 == 0 ? width : num2) + 3;

                return new System.Drawing.Rectangle(minX - 2, minY - 2, width, height + 3);

            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                System.Diagnostics.Debug.WriteLine($"Error in GetWordRectangle: {ex.Message}");
                return System.Drawing.Rectangle.Empty;
            }
        }

        private bool DoesLineContainPhrase(List<XElement> lineWords, string[] inputs)
        {
            if (lineWords.Count < inputs.Length)
                return false;

            // Get the text values from word elements
            var wordTexts = lineWords.Select(w =>
                w.Element("Text")?.Value ?? string.Empty).ToArray();

            // Check if the sequence of input words exists in the line
            for (int i = 0; i <= wordTexts.Length - inputs.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < inputs.Length; j++)
                {
                    if (!string.Equals(wordTexts[i + j], inputs[j], StringComparison.OrdinalIgnoreCase))
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                    return true;
            }
            return false;
        }

        private List<System.Drawing.Rectangle> GetPhraseBoundingRectangles(List<XElement> lineWords, string[] inputs)
        {
            var rectangles = new List<System.Drawing.Rectangle>();

            // Get the text values from word elements
            var wordTexts = lineWords.Select(w =>
                w.Element("Text")?.Value ?? string.Empty).ToArray();

            // Find the starting position of our phrase
            for (int i = 0; i <= wordTexts.Length - inputs.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < inputs.Length; j++)
                {
                    if (!string.Equals(wordTexts[i + j], inputs[j], StringComparison.OrdinalIgnoreCase))
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    // Extract rectangles for the matching words
                    for (int j = 0; j < inputs.Length; j++)
                    {
                        var word = lineWords[i + j];
                        var rect = ExtractRectangleFromWord(word);
                        if (!rect.IsEmpty)
                            rectangles.Add(rect);
                    }
                    break; // Found our phrase, no need to continue
                }
            }

            return rectangles;
        }

        private System.Drawing.Rectangle ExtractRectangleFromWord(XElement word)
        {
            try
            {
                // Parse Location and Size
                var locationElement = word.Element("Location");
                var sizeElement = word.Element("Size");

                if (locationElement != null && sizeElement != null)
                {
                    var locationParts = locationElement.Value.Split(',');
                    var sizeParts = sizeElement.Value.Split(',');

                    if (locationParts.Length == 2 && sizeParts.Length == 2)
                    {
                        if (int.TryParse(locationParts[0].Trim(), out int x) &&
                            int.TryParse(locationParts[1].Trim(), out int y) &&
                            int.TryParse(sizeParts[0].Trim(), out int width) &&
                            int.TryParse(sizeParts[1].Trim(), out int height))
                        {
                            return new System.Drawing.Rectangle(x, y, width, height);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting rectangle: {ex.Message}");
            }

            return System.Drawing.Rectangle.Empty;
        }
    }
}
