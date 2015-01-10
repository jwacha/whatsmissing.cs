using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Windows.Controls;

namespace whatsmissing
{
    public partial class Form1 : Form
    {
        public Dictionary<string, List<int>> indexTable;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private Dictionary<string, List<int>> createIndexTable(string[] stringsToParse)
        {
            Dictionary<string, List<int>> dictToReturn = new Dictionary<string,List<int>>();
            for (int i = 0; i < stringsToParse.Length; i++)
            {
                if (!dictToReturn.ContainsKey(stringsToParse[i])){
                    dictToReturn.Add(stringsToParse[i],new List<int>());
                    dictToReturn[stringsToParse[i]].Add(i);
                }
                else
                {
                    dictToReturn[stringsToParse[i]].Add(i);
                }
            }
            return dictToReturn;
        }

        private bool isSingleQuote(string stringToParse)
        {
            if (stringToParse.Length == 1 && (stringToParse == "\"" || stringToParse == "“" || stringToParse == "”"))
            {
                return true;
            }
            return false;
        }

        private bool isQuoteOnFront(string stringToParse)
        {
            if (isSingleQuote(stringToParse.Substring(0,1))){
                return true;
            }
            return false;
        }

        private bool isQuoteOnEnd(string stringToParse)
        {
            return isSingleQuote(stringToParse.Substring(stringToParse.Length-1,1));
        }

        private bool isQuoteOnBothSides(string stringToParse)
        {
            return (isQuoteOnEnd(stringToParse) && isQuoteOnFront(stringToParse));
        } // I'm keeping this in because it's beautiful even though I'm not using it. Fuck you.

        static string printMatches(bool[] matches, string[] primarySource)
        {
            string toPrint = "";
            for (int i = 0; i < matches.Length; i++ )
            {
                if (matches[i])
                {
                    
                    toPrint = toPrint + primarySource[i];
                }// ends if
            }// ends for
            return toPrint;
        }

        private void compare_Click(object sender, EventArgs e)
        {
            string articleText = article.Text;
            string primarySourceText = primarySource.Text;
            string pattern = @"\p{P}";
            Regex regex = new Regex(pattern);
            primarySourceText = regex.Replace(primarySourceText,"");
            articleText = regex.Replace(articleText, "");
            string[] cleanPrimarySource = Regex.Split(primarySourceText.Trim(), @"\s+"); // to be crossed out and printed.
            string[] primarySourceArray = Regex.Split(primarySourceText.Trim().ToLower(), @"\s+");
            string[] articleTextArray = Regex.Split(articleText.Trim().ToLower(), @"\s+");
            bool[] matches = new bool[primarySourceArray.Length];
            for (int i = 0; i < matches.Length; i++)
            {
                matches[i] = false;
            }
            //string[] articleTextArray = articleText.Split(' ');
            //articleText.Split();
            indexTable = createIndexTable(primarySourceArray);
            for (int positionInArticle = 0; positionInArticle < articleTextArray.Length; positionInArticle++)
            {
                if (indexTable.ContainsKey(articleTextArray[positionInArticle]))
                {
                    // we want to get all the indexes.
                    foreach(int positionInPrimarySource in indexTable[articleTextArray[positionInArticle]]){
                        //MessageBox.Show(positionInPrimarySource.ToString());
                        int possibleMatchSize = 1;
                        string segmentInPrimary = "";
                        string segmentInArticle = "";
                        int editDistance = 10000;
                        do {
                            segmentInPrimary = string.Join("",primarySourceArray,positionInPrimarySource,possibleMatchSize);
                            segmentInArticle = string.Join("",articleTextArray,positionInArticle,possibleMatchSize);
                            //MessageBox.Show("primary: " + segmentInPrimary + " " + "article: " + segmentInArticle);
                            editDistance = lDistance(segmentInArticle,segmentInPrimary);
       
                            if (possibleMatchSize > 3 && editDistance < 5){
                                for (int i = positionInPrimarySource; i < positionInPrimarySource + possibleMatchSize && i < matches.Length; i++)
                                {
                                    matches[i] = true;
                                    //MessageBox.Show("match");
                                }
                            }
                            possibleMatchSize++;
                        } while(editDistance < 5 && positionInArticle + possibleMatchSize < articleTextArray.Length && positionInPrimarySource + possibleMatchSize < primarySourceArray.Length);

                    }// we get 

                }// ends if
            }// ends for

            //printArticleText(primarySourceArray);

            output.Text = printMatches(matches,primarySourceArray);

            /*
            foreach(string key in indexTable.Keys){
                output.Text = output.Text + " " + key + "   " + string.Join(",",indexTable[key].ToArray());
            }
            */

            /*
            for (int i = 0; i < articleTextArray.Length; i++)
            {
                if(isSingleQuote(articleTextArray[i])){
                    isInQuotes = !isInQuotes;
                } else if(isQuoteOnBothSides(articleTextArray[i])) {

                }
                else if (isQuoteOnEnd(articleTextArray[i]))
                {

                }
                else if (isQuoteOnFront(articleTextArray[i]))
                {

                }
            }// ends loop.
             */


            //string[] segments = articleText.Split("\"“”".ToCharArray());
            //articleWord[] tokenizedArticle = new articleWord[articleTextArray.Length];



            /*
            Boolean quoteStarts = false;
            quoteStarts = isSingleQuote(articleText.Substring(0,1));
            List<string> segmentsToParse = new List<string>();
            if(quoteStarts){
                bool addThis = true;
                for(int i = 0; i < segments.Length; i++){
                    if (addThis)
                    {
                        segmentsToParse.Add(segments[i]);
                    }
                    addThis = !addThis;
                }// ends for
            } else {
                bool addThis = false;
                for (int i = 0; i < segments.Length; i++)
                {
                    if (addThis)
                    {
                        segmentsToParse.Add(segments[i]);
                    }
                    addThis = !addThis;
                }
            }
            printArticleText(segmentsToParse.ToArray<string>());
             */

        }

        private void printArticleText(string[] textToPrint)
        {
            string stringToPrint = "";
            for (int i = 0; i < textToPrint.Length; i++)
            {
                stringToPrint = stringToPrint + textToPrint[i];
                stringToPrint = stringToPrint + "             ";
            }
            output.Text = stringToPrint;
        }// ends method

        
        public static Int32 lDistance(String a, String b)
        {

            if (string.IsNullOrEmpty(a))
            {
                if (!string.IsNullOrEmpty(b))
                {
                    return b.Length;
                }
                return 0;
            }

            if (string.IsNullOrEmpty(b))
            {
                if (!string.IsNullOrEmpty(a))
                {
                    return a.Length;
                }
                return 0;
            }

            Int32 cost;
            Int32[,] d = new int[a.Length + 1, b.Length + 1];
            Int32 min1;
            Int32 min2;
            Int32 min3;

            for (Int32 i = 0; i <= d.GetUpperBound(0); i += 1)
            {
                d[i, 0] = i;
            }

            for (Int32 i = 0; i <= d.GetUpperBound(1); i += 1)
            {
                d[0, i] = i;
            }

            for (Int32 i = 1; i <= d.GetUpperBound(0); i += 1)
            {
                for (Int32 j = 1; j <= d.GetUpperBound(1); j += 1)
                {
                    cost = Convert.ToInt32(!(a[i - 1] == b[j - 1]));

                    min1 = d[i - 1, j] + 1;
                    min2 = d[i, j - 1] + 1;
                    min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }

            return d[d.GetUpperBound(0), d.GetUpperBound(1)];
        }
        public class articleWord
        {

            public string word;
            public Boolean isInQuotes;
            public articleWord(string newWord, Boolean newIsInQuotes)
            {
                newWord.ToLower();
                word = new string(newWord.Where(c => !char.IsPunctuation(c)).ToArray());
                isInQuotes = newIsInQuotes;
            }
        }
    }
}
