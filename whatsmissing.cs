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


namespace whatsmissing
{
    public partial class Form1 : Form
    {
        public List<string> debug;
        public Dictionary<string, List<int>> indexTable;
        public string HTMLToPublish;
        public Form1()
        {
            InitializeComponent();
            debug = new List<string>();
            webBrowser.DocumentText = "<body bgcolor=\"silver\"></body>";
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

        private bool isSingleLetterWord(string stringToParse)
        {
            return (string.Equals(stringToParse,"a") || string.Equals(stringToParse,"i"));
        }

        static string printMatches(bool[] matches, string[] primarySource)
        {
            string toPrint = "";
            for (int i = 0; i < matches.Length; i++ )
            {
                if (matches[i])
                {  
                    toPrint = toPrint + primarySource[i] + " ";
                }// ends if
            }// ends for
            return toPrint;
        }

        static public bool[] removeFalsePositives(bool[] boolsToCheck)
        {
            bool trail0 = false;
            bool trail1 = true;
            bool trail2 = true;
            bool trail3 = true;
            for (int i = 0; i < boolsToCheck.Length; i++)
            {
                trail3 = trail2;
                trail2 = trail1;
                trail1 = trail0;
                trail0 = boolsToCheck[i];
                if (!trail0 && trail1 && !trail2){
                    boolsToCheck[i - 1] = false;
                } else if (!trail0 && trail1 && trail2 && !trail3 && i > 3){
                    boolsToCheck[i - 1] = false;
                    boolsToCheck[i - 2] = false;
                }
            }

            return boolsToCheck;
        }

        static public string printAsHtml(bool[] matches, string[] primarySource)
        {
            string openStrike = "<span style=\"color:BlanchedAlmond; background-color:grey;\">";
            string closeStrike = "</span>";
            string toReturn = "";
            Boolean previous = true;
            for (int i = 0; i < matches.Length; i++)
            {
                if(!previous && matches[i]){
                    toReturn = toReturn + closeStrike;
                }

                if (previous && !matches[i]){
                    toReturn = toReturn + openStrike;
                }
                toReturn = toReturn + " " + primarySource[i];
                previous = matches[i];
            }
            return toReturn;
        }

        private void compare_Click(object sender, EventArgs e)
        {
            if (article.Text != "" && primarySource.Text != "")
            {
                string articleText = article.Text;
                string primarySourceText = primarySource.Text;
                string[] cleanPrimarySource = Regex.Split(primarySourceText.Trim(), @"\s+"); // to be crossed out and printed.
                string pattern = @"\p{P}";
                Regex regex = new Regex(pattern);
                primarySourceText = regex.Replace(primarySourceText, "");
                articleText = regex.Replace(articleText, "");
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
                        foreach (int positionInPrimarySource in indexTable[articleTextArray[positionInArticle]])
                        {
                            //MessageBox.Show(positionInPrimarySource.ToString());
                            int possibleMatchSize = 1;
                            string segmentInPrimary = "";
                            string segmentInArticle = "";
                            int editDistance = 10000;
                            do
                            {
                                segmentInPrimary = string.Join("", primarySourceArray, positionInPrimarySource, possibleMatchSize);
                                segmentInArticle = string.Join("", articleTextArray, positionInArticle, possibleMatchSize);
                                //string lastWordInPrimary = primarySourceArray[positionInPrimarySource + possibleMatchSize];
                                int safeIndexInPrimary = positionInPrimarySource + possibleMatchSize;
                                int safeIndexInArticle = positionInArticle + possibleMatchSize;
                                if (safeIndexInPrimary >= primarySourceArray.Length)
                                {
                                    safeIndexInPrimary = primarySourceArray.Length - 1;
                                }
                                if (safeIndexInArticle >= articleTextArray.Length)
                                {
                                    safeIndexInArticle = articleTextArray.Length - 1;
                                }
                                string lastWordInPrimary = primarySourceArray[safeIndexInPrimary - 1];
                                //MessageBox.Show(lastWordInPrimary);
                                string lastWordInArticle = articleTextArray[safeIndexInArticle - 1];
                                //MessageBox.Show("primary: " + segmentInPrimary + " " + "article: " + segmentInArticle);
                                editDistance = lDistance(segmentInArticle, segmentInPrimary);

                                if (possibleMatchSize >= 3 && (editDistance < 2 && // changed positionInArticle
                                    (lastWordInPrimary.Length + lastWordInArticle.Length != 3 &&
                                    primarySourceArray[positionInPrimarySource + possibleMatchSize].Length > editDistance)) ||
                                    (isSingleLetterWord(lastWordInArticle) && (string.Equals(lastWordInArticle, lastWordInPrimary))))
                                {
                                    if (string.Equals(lastWordInArticle, "in") || string.Equals(lastWordInPrimary, "in"))
                                    {
                                        //MessageBox.Show("article: "+lastWordInArticle+"primary: "+lastWordInPrimary);
                                    }
                                    for (int i = positionInPrimarySource; i < positionInPrimarySource + possibleMatchSize && i < matches.Length; i++)
                                    {
                                        matches[i] = true;
                                        //   debug.Add(segmentInArticle + ":" + segmentInPrimary + ":" + i.ToString() + ",   ");
                                        //MessageBox.Show("match");
                                    }// ends for
                                }// ends if
                                possibleMatchSize++;
                            } while (editDistance < 3 && positionInArticle + possibleMatchSize < articleTextArray.Length && positionInPrimarySource + possibleMatchSize < primarySourceArray.Length);
                            // this catches the last word.
                            if (string.Equals(articleTextArray[articleTextArray.Length - 1], primarySourceArray[primarySourceArray.Length - 1]))
                            {
                                matches[matches.Length - 1] = true;
                                // we need to make a sequence backward from the end. Then we need to 
                            }
                        }// we get 
                    }// ends if
                }// ends for

                string LastWordOfLastSegment = articleTextArray[articleTextArray.Length-1];
                //MessageBox.Show(LastWordOfLastSegment);
                string lastSegmentToCheck = string.Join("",articleTextArray,articleTextArray.Length -3, 3);
                //MessageBox.Show(lastSegmentToCheck);
                if (indexTable.ContainsKey(LastWordOfLastSegment)){
                foreach(int indexToExamine in indexTable[LastWordOfLastSegment]){
                    if(indexToExamine > 3){
                        string backWardsSegment = string.Join("",primarySourceArray,indexToExamine-2,3);
                        if(string.Equals(backWardsSegment, lastSegmentToCheck)){
                            matches[indexToExamine] = true;
                        }
                    }
                }
                }
                //printArticleText(primarySourceArray);
                //.outputMessageBox.Show("matches" + matches.Length + "primarysource" + cleanPrimarySource.Length);
                matches = removeFalsePositives(matches);
                string myHTML = printAsHtml(matches, cleanPrimarySource);
                //output.Text = myHTML;
                //string stringToPutInBox = "";
                //foreach (string stringToAdd in debug)
                //{
                //    stringToPutInBox = stringToPutInBox + stringToAdd;
                //}
                webBrowser.DocumentText = myHTML;
                //webBrowser.DocumentText = stringToPutInBox;
                HTMLToPublish = myHTML;
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
        }

        private void printArticleText(string[] textToPrint)
        {
            string stringToPrint = "";
            for (int i = 0; i < textToPrint.Length; i++)
            {
                stringToPrint = stringToPrint + textToPrint[i];
                stringToPrint = stringToPrint + "             ";
            }
            //output.Text = stringToPrint;
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

        private void publish_Click(object sender, EventArgs e)
        {
            string stringToPublish = HTMLToPublish;
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "html file|*.html";
            saveFile.Title = "Save the HTML to publish on web.";
            saveFile.ShowDialog();
            string nameOfSave = saveFile.FileName;
            try
            {
                System.IO.StreamWriter writeStream = new System.IO.StreamWriter(nameOfSave);
                writeStream.Write(stringToPublish);
                writeStream.Close();
            }
            catch
            {
                // don't do anything.
            }
        }
    }
}
