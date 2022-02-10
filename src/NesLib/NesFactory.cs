using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib
{
    public class NesFactory
    {
        public static INes New()
        {
            return new Nes();
        }
    }
}
