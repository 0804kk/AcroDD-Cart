using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcroDD_Cart
{
    class PIDController
    {

        private double pGain;
        private double iGain;
        private double dGain;

        private double p;
        private double i;
        private double d;
        private double diff;
        private double preDiff;

        //public double PGain { get => pGain; set => pGain = value; }
        //public double IGain { get => iGain; set => iGain = value; }
        //public double DGain { get => dGain; set => dGain = value; }
        public PIDController(double _pGain, double _iGain, double _dGain)
        {
            pGain = _pGain;
            iGain = _iGain;
            dGain = _dGain;
        }
        public double GetPIDValue(double target, double control,double dt)
        {
            diff = target - control;
            p = diff * pGain;
            i += diff * iGain * dt;
            d = (diff - preDiff) / dt * dGain;
            preDiff = diff;
            return p + i + d;
        }
    }
}
