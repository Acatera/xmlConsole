using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace xmlConsole
{
    class ParserWorker
    {
        /** Numele fisierului XML*/
        protected static string xmlFile = "";

        /** Categoriile ce trebuie exportate din fisierul XML*/
        protected static string exportPath = "";

        /** Definitiile campurilor din tabela Drugs*/
        List<Field> drugsFields = new List<Field>();

        /** Definitiile campurilor din tabela Drugs*/
        List<Field> physFields = new List<Field>();

        /** Definitiile campurilor din tabela Drugs*/
        List<Field> phySpecFields = new List<Field>();

        /** Definitiile campurilor din tabela Drugs*/
        List<Field> copDrugsFields = new List<Field>();

        /** Definitiile campurilor din tabela Drugs*/
        List<Field> copActSubFields = new List<Field>();

        /** Definitiile campurilor din tabela Drugs*/
        List<Field> prescriptionDrugsHeader = new List<Field>();
        List<Field> prescriptionDrugsBody = new List<Field>();

        MappingHelper helper = new MappingHelper();

        public ParserWorker(String xmlFileName, String dbfFileName)
        {
            if (xmlFileName != "")
                xmlFile = xmlFileName;
            //xmlFile = xmlFileName != "" ? xmlFile : @"D:\2.txt";
            if (dbfFileName != "")
                exportPath = dbfFileName;
            init();
        }

        public bool doWork_XmlDocument()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFile);
            
            XmlNodeList recipes = doc.SelectNodes("prescriptions");


            return true;
        }

        public bool doWork()
        {
            string buffer = "";
            XDocument doc = XDocument.Load(xmlFile);

            XNode node = doc.FirstNode;
            XNode node2 = node2.FirstNode;
            XNode node3 = doc.FirstNode;
            IEnumerable<XElement> root = doc.Descendants("report");
            
            //XElement root = doc.Element("report");
            IEnumerable<XNode> nodes = doc.DescendantNodes();
            IEnumerable<XElement> elements = (from el in doc.Descendants("prescriptions")
                                            select el);

            IEnumerable<string> output = (from c in doc.Descendants("prescriptions")
                                 select c.Attribute("prescriptionRefID").Value);


            foreach (XElement recipt in doc.Descendants("prescriptions"))
            {
                string reciptHeader = "";
                string reciptDetails = "";
                foreach (Field f in prescriptionDrugsHeader)
                {
                    reciptHeader += recipt.Attribute(f.xmlTag).Value.PadLeft(f.fieldLength);
                }
                foreach (XElement drug in recipt.Descendants())
                {
                    foreach (Field f in prescriptionDrugsBody)
                    {
                        reciptDetails += drug.Attribute(f.xmlTag).Value.PadLeft(f.fieldLength);
                    }
                }
                buffer += " " + reciptHeader + reciptDetails;
            }
            return true;
        }

        public bool doWork2()
        {
            List<Field> fieldList = new List<Field>();
            fieldList.AddRange(prescriptionDrugsHeader);
            fieldList.AddRange(prescriptionDrugsBody);

            byte[] test = helper.getDBFHeader(fieldList);

            if (xmlFile == "" || exportPath == "")
                return false;

            FileStream saver = new FileStream(exportPath, FileMode.Create, FileAccess.Write);
            saver.Write(test, 0, test.Length);

            if (xmlFile != "" && File.Exists(xmlFile))
            {


                XmlReader reader = XmlReader.Create(xmlFile);

                string buffer = "";

                reader.Read();
                //reader.ReadStartElement("report");
                reader.ReadToFollowing("prescriptions");
                string output = "";
                foreach (Field f in prescriptionDrugsHeader)
                {
                    if (reader.MoveToAttribute(f.xmlTag))
                    {
                        output += reader.Value.PadLeft(f.fieldLength);
                    }
                    else
                    {
                        output += "".PadLeft(f.fieldLength);
                    }
                }
                buffer = output;
                Console.Out.WriteLine(output);

                reader.ReadToFollowing("prescriptionDrugs");
            }

            return true;


            if (xmlFile != "") //&& categoriesToGet.Length == 5 && categoriesToGet != "00000")
            {
                //FileStream fileStream = File.Open(xmlFile, FileMode.Open, FileAccess.Read, FileShare.None);
                StreamReader streamReader = new StreamReader(xmlFile);

                string recipeHeader = "";
                string block = "";
                int noRecords = -1;

                streamReader.ReadLine();
                streamReader.ReadLine();
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine().Trim().Replace("</prescriptionDrugs>", "");

                    if (String.Compare(line, 0, "<prescriptions", 0, 14, true) == 0)
                    {
                        recipeHeader = processString(prescriptionDrugsHeader, line);
                    }
                    else if (String.Compare(line, 0, "</prescriptions>", 0, 16, true) == 0)
                    {
                        byte[] toSave = Encoding.ASCII.GetBytes(block);
                        saver.Write(toSave, 0, toSave.Length);
                        recipeHeader = "";
                        block = "";
                    }
                    else if (line != "")
                    {
                        block += " " + recipeHeader + processString(prescriptionDrugsBody, line);
                        noRecords++;
                    }

                }
                streamReader.Close();
                saver.WriteByte(13);
                saver.Seek(4, SeekOrigin.Begin);
                saver.WriteByte((byte)(noRecords & 255));
                saver.WriteByte((byte)((noRecords >> 8) & 255));
                saver.WriteByte((byte)((noRecords >> 16) & 255));
                saver.WriteByte((byte)((noRecords >> 24) & 255));
                saver.Close();
            }
            return true;
        }

        protected string processString(List<Field> fieldDefs, string line)
        {
            line = line.Trim();
            if (line != "")
            {
                string output = "";
                Regex entries = new Regex("(\\S+)=[\"']?((?:.(?![\"']?\\s+(?:\\S+)=|[>\"']))+.)[\"']?", RegexOptions.Compiled);
                MatchCollection mc = entries.Matches(line);

                foreach (Field field in fieldDefs)
                {
                    bool wasMatched = false;
                    foreach (Match match in mc)
                    {
                        if (match.Value.Split('=')[0] == field.xmlTag)
                        //if (helper.getDBFFieldName(prescriptionDrugs, match.Value.Split('=')[0]) != "")
                        {
                            string rawFieldData = match.Value.Split('=')[1].Replace("\"", "").PadLeft(field.fieldLength);
                            output += rawFieldData;
                            wasMatched = true;
                            break;
                        }
                    }
                    if (!wasMatched)
                    {
                        output += @"".PadLeft(field.fieldLength);
                    }
                }
                return output;
            }
            return "";
        }

        protected void init()
        {
            prescriptionDrugsHeader.Add(new Field("statusCode", "C", 2, "statusCode"));
            prescriptionDrugsHeader.Add(new Field("statusFlag", "C", 5, "statusFlag"));
            prescriptionDrugsHeader.Add(new Field("prescRefID", "C", 20, "prescriptionRefID"));
            prescriptionDrugsHeader.Add(new Field("fractionNo", "C", 2, "fractionNo"));
            prescriptionDrugsHeader.Add(new Field("invoiceNo", "C", 10, "invoiceNo"));
            prescriptionDrugsHeader.Add(new Field("invDate", "C", 10, "invoiceDate"));
            prescriptionDrugsHeader.Add(new Field("receipt", "C", 15, "receipt"));
            prescriptionDrugsHeader.Add(new Field("issuedType", "C", 2, "issuedType"));
            prescriptionDrugsHeader.Add(new Field("no", "C", 10, "no"));
            prescriptionDrugsHeader.Add(new Field("series", "C", 10, "series"));

            prescriptionDrugsBody.Add(new Field("drugInvDat", "C", 10, "drugInvoiceDate"));
            prescriptionDrugsBody.Add(new Field("drugInvNo", "C", 10, "drugInvoiceNo"));
            prescriptionDrugsBody.Add(new Field("drugRefID", "C", 20, "drugRefID"));

            drugsFields.Add(new Field("code", "C", 10, "code"));
            drugsFields.Add(new Field("name", "C", 90, "name"));
            drugsFields.Add(new Field("isNarcotic", "C", 1, "isNarcotic"));
            drugsFields.Add(new Field("isFraction", "C", 5, "isFractional"));
            drugsFields.Add(new Field("isSpecial", "C", 5, "isSpecial"));
            drugsFields.Add(new Field("hasBioEchi", "C", 5, "hasBioEchiv"));
            drugsFields.Add(new Field("qtyPerPack", "C", 5, "qtyPerPackage"));
            drugsFields.Add(new Field("pricePerPa", "C", 15, "pricePerPackage"));
            drugsFields.Add(new Field("wholeSaleP", "C", 15, "wholeSalePricePerPackage"));
            drugsFields.Add(new Field("prescripti", "C", 10, "prescriptionMode"));
            drugsFields.Add(new Field("validFrom", "C", 10, "validFrom"));
            drugsFields.Add(new Field("validTo", "C", 10, "validTo"));
            drugsFields.Add(new Field("activeSubs", "C", 60, "activeSubstance"));
            drugsFields.Add(new Field("concentrat", "C", 60, "concentration"));
            drugsFields.Add(new Field("pharmaceut", "C", 60, "pharmaceuticalForm"));
            drugsFields.Add(new Field("company", "C", 50, "company"));
            drugsFields.Add(new Field("country", "C", 5, "country"));
            drugsFields.Add(new Field("atc", "C", 8, "atc"));
            drugsFields.Add(new Field("presentati", "C", 60, "presentationMode"));

            physFields.Add(new Field("pid", "C", 15, "pid"));
            physFields.Add(new Field("name", "C", 40, "name"));
            physFields.Add(new Field("stencil", "C", 10, "stencil"));
            physFields.Add(new Field("validFrom", "C", 10, "validFrom"));
            physFields.Add(new Field("validTo", "C", 10, "validTo"));

            phySpecFields.Add(new Field("stencil", "C", 10, "stencil"));
            phySpecFields.Add(new Field("contractNo", "C", 20, "contractNo"));
            phySpecFields.Add(new Field("insuranceH", "C", 6, "insuranceHouse"));
            phySpecFields.Add(new Field("contractTy", "C", 5, "contractType"));
            phySpecFields.Add(new Field("physicianT", "C", 1, "physicianType"));
            phySpecFields.Add(new Field("speciality", "C", 20, "specialityCode"));
            phySpecFields.Add(new Field("validFrom", "C", 10, "validFrom"));
            phySpecFields.Add(new Field("validTo", "C", 10, "validTo"));

            copDrugsFields.Add(new Field("copaymentL", "C", 2, "copaymentListType"));
            copDrugsFields.Add(new Field("drug", "C", 10, "drug"));
            copDrugsFields.Add(new Field("nhpCode", "C", 5, "nhpCode"));
            copDrugsFields.Add(new Field("MaxPrice", "C", 15, "maxPrice"));
            copDrugsFields.Add(new Field("MaxPriceUT", "C", 15, "maxPriceUT"));
            copDrugsFields.Add(new Field("CopaymentV", "C", 15, "copaymentValue"));
            copDrugsFields.Add(new Field("Copayment9", "C", 15, "copaymentValue90"));
            copDrugsFields.Add(new Field("wholeSaleP", "C", 15, "wholeSalePrice"));
            copDrugsFields.Add(new Field("referenceP", "C", 15, "referencePrice"));
            copDrugsFields.Add(new Field("specialLaw", "C", 5, "specialLaw"));
            copDrugsFields.Add(new Field("validFrom", "C", 10, "validFrom"));
            copDrugsFields.Add(new Field("validTo", "C", 10, "validTo"));
            copDrugsFields.Add(new Field("diseaseCod", "C", 5, "diseaseCode"));
            copDrugsFields.Add(new Field("overValue", "C", 2, "overValue"));
            copDrugsFields.Add(new Field("classifIn", "C", 1, "classifInsulin"));

            copActSubFields.Add(new Field("copaymentL", "C", 2, "copaymentListType"));
            copActSubFields.Add(new Field("activeSubs", "C", 60, "activeSubstance"));
            copActSubFields.Add(new Field("aTC", "C", 8, "aTC"));
            copActSubFields.Add(new Field("needApprov", "C", 1, "needApproval"));
            copActSubFields.Add(new Field("validFrom", "C", 10, "validFrom"));
            copActSubFields.Add(new Field("validTo", "C", 10, "validTo"));
            copActSubFields.Add(new Field("diseasecat", "C", 5, "diseasecategory"));
        }
    }
}
