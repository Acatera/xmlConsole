using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xmlConsole
{
    class MappingHelper
    {
        public static bool shouldGetPhys = false;
        public static bool shouldGetPhySpec = false;
        public static bool shouldGetDrugs = false;
        public static bool shouldGetCopDrugs = false;
        public static bool shouldGetCopActSub = false;

        /*
        public void MappingHelper(string[] args)
        {
            if (args[0] == "1") shouldGetPhys = true;
            if (args[0] == "1") shouldGetPhys = true;
            if (args[0] == "1") shouldGetPhys = true;
            if (args[0] == "1") shouldGetPhys = true;
            if (args[0] == "1") shouldGetPhys = true;

        }
        */

        public string getDBFFieldName(List<Field> fieldDefs, string xmlFieldName)
        {
            foreach (Field field in fieldDefs)
            {
                if (field.xmlTag == xmlFieldName)
                    return field.name;
            }
            return "";
        }

        public int getDBFFieldLength(List<Field> fieldDefs, string xmlFieldName)
        {
            foreach (Field field in fieldDefs)
            {
                if (field.xmlTag == xmlFieldName)
                    return field.fieldLength;
            }
            return 0;
        }

        /** Returns a byte array containing the DBF header */
        public byte[] getDBFHeader(List<Field> fieldDefs)
        {
            byte[] descArray = new byte[32 * fieldDefs.Count];

            int descArrayLength = 0;
            foreach (Field field in fieldDefs)
            {
                System.Buffer.BlockCopy(field.getDescriptor(), 0, descArray, fieldDefs.IndexOf(field) * 32, 32);
                descArrayLength = descArrayLength + field.fieldLength;
            }

            descArrayLength++;

            byte[] header = new byte[33 + fieldDefs.Count * 32];

            header[0] = 3; //dBase 4
            header[1] = (byte)(DateTime.Today.Year % 100); //YY
            header[2] = (byte)DateTime.Today.Month; //MM
            header[3] = (byte)DateTime.Today.Day; //DD

            header[8] = (byte)((33 + fieldDefs.Count * 32) & 255);  //bytes in header (two bytes)
            header[9] = (byte)((33 + fieldDefs.Count * 32) >> 8);

            header[10] = (byte)(descArrayLength & 255);  //bytes in record (two bytes)
            header[11] = (byte)(descArrayLength << 8);

            System.Buffer.BlockCopy(descArray, 0, header, 32, descArray.Length);
            header[header.Length - 1] = 13;
            return header;
        }

    }
}
