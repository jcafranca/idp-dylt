using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.DataTypes
{
    public class RegexInfo
    {
        public class regexPatternsClss
        {
            public List<string> COMPANYNAME { get; set; } = new List<string>();
            public List<string> ADDRESS { get; set; } = new List<string>();
            public List<string> DATE_ { get; set; } = new List<string>();
            public List<string> PHONE { get; set; } = new List<string>();
            public List<string> CONTACTNAME { get; set; } = new List<string>();
            public List<string> ZIPCODE { get; set; } = new List<string>();
        }
        public regexPatternsClss regexPatterns { get; set; } = new regexPatternsClss();
        public RegexInfo()
        {
            InitializeRegexPatterns();
        }


        public void InitializeRegexPatterns()
        {
            regexPatterns.COMPANYNAME.Add("NAME");

            regexPatterns.ADDRESS.AddRange(new List<string>
                {
                    "PO BOX \\d+|P O BOX \\d+|P O BON \\d+|P O \\d+",
                    "SUITE [A-Z0-9]+",
                    "^\\d{1,5}[ ](?:[A-Za-z0-9.#-]+[ ]?)*",
                    "ROAD|BLVD|UNIT|ADDRESS|INDUSTRIAL",
                    "Pd BOX \\d+"
                });

            regexPatterns.DATE_.AddRange(new List<string>
                {
                    "\\d{1,2}/\\d{1,2}/\\d{4}",
                    "\\d{1,2}/\\d{1,2}/\\d{1,2}"
                });

            regexPatterns.PHONE.AddRange(new List<string>
                {
                    "\\d{4} \\d{3} \\d{4}",
                    "\\d{3}.\\d{3}.\\d{4}",
                    "\\+\\d{1}-\\(\\d{3}\\)-\\d{3}-\\d{4}",
                    "\\d{3}-\\d{3}-\\d{4}",
                    "\\d{3} \\d{4} \\d{3}",
                    "\\d{3} \\d{3} \\d{4}",
                    "\\(\\d{3}\\) \\d{4}-\\d{3}",
                    "\\(\\d{3}\\) \\d{3} - \\d{4}",
                    "\\d{11}|\\d{10}",
                    "\\d{3}\\ \\d{2} \\d{1} \\d{4}",
                    "X\\d{3} \\d{3} \\d{3} \\d{4}",
                    "\\(\\d{3}\\)-\\d{3}-\\d{4}",
                    "\\d{3} \\d{7}",
                    "\\(\\d{3}\\) \\d{3}-\\d{4}",
                    "\\(\\d{3}\\)\\d{3}-\\d{4}",
                    "\\(\\d{3}\\) \\d{3}\\-\\d{4}"
                });

            regexPatterns.ZIPCODE.AddRange(new List<string>
                {
                    // CANADIAN ZIPCODE
                    "[ABCEGHJKLMNPRSTVXY]\\d[ABCEGHJ-NPRSTV-Z][ ]?\\d[ABCEGHJ-NPRSTV-Z]\\d|[ABCEGHJKLMNPRSTVXY]\\d [ABCEGHJ-NPRSTV-Z][ ]\\d[ABCEGHJ-NPRSTV-Z]\\d",
                    // USA ZIPCODE
                    "\\d{5}|\\d{4}|\\d{2} \\d{3}",
                    // MEXICO ZIPCODE
                    "\\d{5}[ABCEGHJKLMNPRSTVXY]"
                });
        }

    }
}
