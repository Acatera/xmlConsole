using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xmlConsole
{
    class Field
    {
        public string name;
        public string fieldType;
        public byte fieldLength;
        public string xmlTag;

        public Field(string name, string fieldType, byte fieldLength, string xmlTag)
        {
            this.name = name;
            this.fieldType = fieldType;
            this.fieldLength = fieldLength;
            this.xmlTag = xmlTag;
        }

        public byte[] getDescriptor()
        {
            byte[] desc = new byte[32];

            System.Buffer.BlockCopy(Encoding.ASCII.GetBytes(name), 0, desc, 0, name.Length);
            //desc[11] = 110;//Encoding.ASCII.GetBytes(fieldType);
            desc[11] = Encoding.ASCII.GetBytes(fieldType)[0];
            desc[16] = fieldLength;
            return desc;
        }
    }
}
