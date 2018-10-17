using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LandisPro.Harvest
{
    class HarvestEvent
    {
        public const int EVENT_ONE_PASS_STAND_FILLING_REGIME = 9;
        public const int EVENT_PERIODIC_STAND_FILLING_REGIME = 10;
        public const int EVENT_TWO_PASS_STAND_FILLING_REGIME = 11;
        public const int EVENT_ONE_PASS_STAND_SPREADING_REGIME = 4;
        public const int EVENT_TWO_PASS_STAND_SPREADING_REGIME = 5;
        public const int EVENT_GROUP_SELECTION_REGIME = 6;
        public const int EVENT_PERIODIC_TWO_PASS_STAND_FILLING_REGIME = 7;
        public const int EVENT_REPEATING_TWO_PASS_STAND_FILLING_REGIME = 8;
        public const int EVENT_Volume_BA_THINING = 1;
        public const int EVENT_GROUP_SELECTION_REGIME_70 = 2;
        public const int EVENT_STAND_STOCKING_HARVEST = 3;

        private string itsLabel;
        private int itsSequentialId;
        private int userInputID_70;

        public HarvestEvent()
        {
            itsLabel = "";
            SetLabel("none");
            itsSequentialId = 0;
        }

        ~HarvestEvent()
        {
            itsLabel = null;
        }

        public virtual int IsA()
        {
            return 0;
        }

        public virtual int Conditions()
        {
            return 0;
        }

        public virtual void Harvest()
        {

        }

        public virtual void Read(StreamReader inFile)
        {

        }

        public void SetSequentialId(int someId)
        {
            itsSequentialId = someId;
        }

        public int GetSequentialId()
        {
            return itsSequentialId;
        }

        public void SetUserInputId(int someId)
        {
            userInputID_70 = someId;
        }

        public int GetUserInputId()
        {
            return userInputID_70;
        }

        public void SetLabel(string label)
        {
            itsLabel = "";
            itsLabel = label;
        }

        public string GetLabel()
        {
            return itsLabel;
        }
    }
}
