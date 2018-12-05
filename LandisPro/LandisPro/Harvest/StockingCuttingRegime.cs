using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LandisPro.Harvest
{
    class StockingCuttingRegime: HarvestRegime
    {
        public int StandsCut;
        public int SitesCut;
        public int total_reentry_event_instances;
        public StockingCuttingRegime_reentry_event[] StockingCuttingRegime_reentry_event_instances;
        public enum Enum
        {
            START,
            TOHARVEST,
            TOREENTRY,
            DONE,
            PENDING
        }
        private Enum itsState;
        private int itsEntryDecade;
        private int itsFinalDecade;
        private int itsRepeatInterval;
        private int itsTargetCut;
        private double targetProportion;
        private double TargetStocking; //Aug 03 2009
        private double Mininum_Stocking; //May 26 2011
        private int Small0_Large1; //May 26 2011
        private int[] speciesOrder = new int[200]; //May 26 2011
        private int[] flag_plant = new int[200];
        private int[] flag_cut = new int[200];
        private int[] num_TreePlant = new int[200];
        private List<int> itsStands = new List<int>();
        //<Add By Qia on June 02 2012>
        private int itsTargetCut_copy;
        private double targetProportion_copy;
        private double TargetStocking_copy; //Aug 03 2009
        private double Mininum_Stocking_copy; //May 26 2011
        private int Small0_Large1_copy; //May 26 2011
        private int[] speciesOrder_copy = new int[200]; //May 26 2011
        private int[] flag_plant_copy = new int[200];
        private int[] flag_cut_copy = new int[200];
        private int[] num_TreePlant_copy = new int[200];

        public StockingCuttingRegime()
        {
            itsState = Enum.START;
            itsEntryDecade = 0;
            itsFinalDecade = 0;
            itsRepeatInterval = 0;
            itsTargetCut = 0;
        }

        public override int Conditions()
        {
            int passed;
            switch (itsState)
            {
                case Enum.PENDING:
                    if (BoundedPocketStandHarvester.currentDecade == itsEntryDecade)
                    {
                        passed = 1;
                        itsState = Enum.TOHARVEST;
                        send_parameters_to_current(1, -1);
                    }
                    else if (BoundedPocketStandHarvester.currentDecade > itsEntryDecade && (BoundedPocketStandHarvester.currentDecade - itsEntryDecade) % itsRepeatInterval == 0)
                    {
                        passed = 1;
                        itsState = Enum.TOHARVEST;
                        send_parameters_to_current(1, -1);
                    }
                    else
                    {
                        passed = 0;
                    }
                    if (BoundedPocketStandHarvester.currentDecade > itsEntryDecade)
                    {
                        for (int i = 0; i < total_reentry_event_instances; i++)
                        {
                            int inteval_reentry = StockingCuttingRegime_reentry_event_instances[i].itsReentryInteval;
                            if ((BoundedPocketStandHarvester.currentDecade - itsEntryDecade - inteval_reentry) % (itsRepeatInterval) == 0 || (BoundedPocketStandHarvester.currentDecade - itsEntryDecade) - inteval_reentry == 0)
                            {
                                passed = 1;
                                itsState = Enum.TOREENTRY;
                                send_parameters_to_current(0, i);
                            }
                        }
                    }
                    break;
                case Enum.DONE:
                    passed = 0;
                    break;
                default:
                    throw new Exception("Illegal call to stocking::conditions.");
            }
            return passed;
        }

        public override int IsA()
        {
            return EVENT_STAND_STOCKING_HARVEST;
        }

        public override void Harvest()
        {
            StandsCut = 0;
            switch (itsState)
            {
                case Enum.TOHARVEST:
                    base.Harvest();
                    itsState = Enum.PENDING;
                    break;
                case Enum.TOREENTRY:
                    reharvest();
                    itsStands.Clear();
                    itsState = Enum.PENDING;
                    break;
                default:
                    throw new Exception("Illegal call to stocking::harvest.");
            }
        }

        public void reharvest()
        {
            Stand stand = new Stand();
            for (int ii = 0; ii < itsStands.Count; ii++)
            {
                stand.Copy(BoundedPocketStandHarvester.pstands[itsStands[ii]]);
                SitesCut += stand.numberOfActiveSites();
                double stocking_debug = computeStandStocking(stand);
                Ldpoint pt = new Ldpoint();
                int m;
                int k;
                int i;
                int j;
                double TmpStockingS;
                double TmpStockingS_avg;
                double Stocking_toCut;
                double shareCut_ACell;
                TmpStockingS = 0;
                for (StandIterator it = new StandIterator(stand); it.moreSites(); it.gotoNextSite())
                {
                    pt = it.getCurrentSite();
                    i = pt.y;
                    j = pt.x;
                    TmpStockingS += GetStockinginACell(i, j);
                }
                TmpStockingS = TmpStockingS / stand.numberOfActiveSites();
                if (TmpStockingS <= Mininum_Stocking)
                {
                }
                else
                { //Cut trees here
                    StandsCut++;
                    Stocking_toCut = TmpStockingS - TargetStocking;
                    for (StandIterator it = new StandIterator(stand); it.moreSites(); it.gotoNextSite())
                    {
                        pt = it.getCurrentSite();
                        i = pt.y;
                        j = pt.x;
                        shareCut_ACell = GetStockinginACell(i, j) / TmpStockingS * Stocking_toCut;
                        if (shareCut_ACell > 0.0)
                        {
                            CutShareStockinginACell_LifeSpanPercent(i, j, shareCut_ACell);
                            BoundedPocketStandHarvester.pHarvestsites[i, j].harvestType = (short)GetUserInputId();
                            BoundedPocketStandHarvester.pHarvestsites[i, j].lastHarvest = (short)BoundedPocketStandHarvester.currentDecade;
                        }
                        else
                        {
                            //pHarvestsites->SetValueHarvestBA(i,j,shareCut_ACell);
                        }
                    }
                }
            }
        }

        public override int harvestStand(Stand stand)
        {
            //printf("before harvest standID: %d, stocking: %lf\n",stand->getId(),computeStandStocking(stand));
            SitesCut += stand.numberOfActiveSites();
            //printf("sitesinstand:%d SitesCut:%d Target:%d\n",stand->numberOfActiveSites(),SitesCut,itsTargetCut);
            Ldpoint pt = new Ldpoint();
            int m;
            int k;
            int i;
            int j;
            double TmpStockingS;
            double TmpStockingS_avg;
            double Stocking_toCut;
            double shareCut_ACell;
            TmpStockingS = 0;
            for (StandIterator it = new StandIterator(stand); it.moreSites(); it.gotoNextSite())
            {
                pt = it.getCurrentSite();
                i = pt.y;
                j = pt.x;
                TmpStockingS += GetStockinginACell(i, j);
            }
            TmpStockingS = TmpStockingS / stand.numberOfActiveSites();
            //TmpStockingS_avg = TmpStockingS / pCoresites->CellSize/pCoresites->CellSize/stand->numberOfActiveSites()*10000;
            TmpStockingS_avg = TmpStockingS / stand.numberOfActiveSites();
            if (TmpStockingS <= Mininum_Stocking)
            {

            }
            else
            { //Cut trees here

                //printf("Enough to Harvest\n");
                StandsCut++;
                itsStands.Add(stand.getId()); //Add By Qia on June 01 2012
                Stocking_toCut = TmpStockingS - TargetStocking;
                //Stocking_toCut = Stocking_toCut * pCoresites->CellSize * pCoresites->CellSize * stand->numberOfActiveSites()/10000;
                for (StandIterator it = new StandIterator(stand); it.moreSites(); it.gotoNextSite())
                {
                    pt = it.getCurrentSite();
                    i = pt.y;
                    j = pt.x;
                    shareCut_ACell = GetStockinginACell(i, j) / TmpStockingS * Stocking_toCut;
                    if (shareCut_ACell > 0.0)
                    {
                        //CutShareStockinginACell(i,j,shareCut_ACell);
                        //SitesCut++;
                        CutShareStockinginACell_LifeSpanPercent(i, j, shareCut_ACell);
                        BoundedPocketStandHarvester.pHarvestsites[i, j].harvestType = (short)GetUserInputId();
                        BoundedPocketStandHarvester.pHarvestsites[i, j].lastHarvest = (short)BoundedPocketStandHarvester.currentDecade;
                    }
                    else
                    {
                        //pHarvestsites->SetValueHarvestBA(i,j,shareCut_ACell);
                    }
                }
            }
            //return StandsCut;
            //printf("after harvest standID: %d, stocking: %lf\n",stand->getId(),computeStandStocking(stand));
            return 1;
        }

        public override int isHarvestDone()
        {
            if (SitesCut >= itsTargetCut)
            {
                SitesCut = 0;
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public double GetStockinginACell_spec_age(int i, int j, int spec, int age)
        {
            int m = age;
            int k = spec;
            double num_trees = 0; //N
            double Diameters = 0; //D
            double Diameters_square = 0; //D^2
            double x = BoundedPocketStandHarvester.pCoresites.stocking_x_value;
            double y = BoundedPocketStandHarvester.pCoresites.stocking_y_value;
            double z = BoundedPocketStandHarvester.pCoresites.stocking_z_value;
            Landunit l;
            l = BoundedPocketStandHarvester.pCoresites.locateLanduPt(i, j);
            num_trees += BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(k).getTreeNum(m, k);
            Diameters += BoundedPocketStandHarvester.pCoresites.GetGrowthRates(k, m, l.ltID) * BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(k).getTreeNum(m, k) / 2.54;
            Diameters_square += BoundedPocketStandHarvester.pCoresites.GetGrowthRates(k, m, l.ltID) * BoundedPocketStandHarvester.pCoresites.GetGrowthRates(k, m, l.ltID) * BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(k).getTreeNum(m, k) / 2.54 / 2.54;

            return (x * num_trees + y * Diameters + z * Diameters_square) / (BoundedPocketStandHarvester.pCoresites.CellSize * BoundedPocketStandHarvester.pCoresites.CellSize / 4046.86);

        }


        public double CutShareStockinginACell_LifeSpanPercent(int i, int j, double target)
        {
            float[] LifeSpanpercentage = new float[6];
            LifeSpanpercentage[0] = 0.0F;
            LifeSpanpercentage[1] = 0.2F;
            LifeSpanpercentage[2] = 0.4F;
            LifeSpanpercentage[3] = 0.6F;
            LifeSpanpercentage[4] = 0.8F;
            LifeSpanpercentage[5] = 1.0F;
            int[] AgeCohortGroup = new int[BoundedPocketStandHarvester.pCoresites.specNum * 6];
            int[] AgeArraySmall;
            int[] AgeArrayLarge;
            int age_largest = 0;
            int i_count;
            int m;
            int k;
            int m_count;
            int flag_cut_anyspecie = 0;
            double TmpStockingS = 0;
            double tempStocking;
            double StockingcutSpecie;
            double tempBA;
            double singleTreeBA;
            int treeNum_save;
            int treeNum_original;
            double Stocking_actual_cut_cell = 0;

            Landunit l;
            l = BoundedPocketStandHarvester.pCoresites.locateLanduPt(i, j);

            for (k = 0; k < BoundedPocketStandHarvester.pCoresites.specNum; k++)
            {
                int temp = BoundedPocketStandHarvester.pCoresites[i, j].specAtt(speciesOrder[k]).longevity / BoundedPocketStandHarvester.pCoresites.TimeStep;
                if (age_largest < temp)
                {
                    age_largest = temp;
                }
                for (i_count = 0; i_count < 6; i_count++)
                {
                    AgeCohortGroup[(speciesOrder[k] - 1) * 6 + i_count] = (int) (temp * LifeSpanpercentage[i_count]);
                }
            }
            AgeArraySmall = new int[5 * BoundedPocketStandHarvester.pCoresites.specNum * (age_largest / 5 + 1)];
            AgeArrayLarge = new int[5 * BoundedPocketStandHarvester.pCoresites.specNum * (age_largest / 5 + 1)];
            for (i_count = 0; i_count < 5; i_count++)
            {
                for (k = 0; k < BoundedPocketStandHarvester.pCoresites.specNum; k++)
                {
                    int temp = BoundedPocketStandHarvester.pCoresites[i, j].specAtt(speciesOrder[k]).longevity / BoundedPocketStandHarvester.pCoresites.TimeStep;
                    int tempstart = AgeCohortGroup[(speciesOrder[k] - 1) * 6 + i_count] + 1;
                    int tempend = AgeCohortGroup[(speciesOrder[k] - 1) * 6 + i_count + 1];
                    int value = tempstart;
                    for (int tempcount = 0; tempcount < age_largest / 5 + 1; tempcount++)
                    {
                        int pos = i_count * (BoundedPocketStandHarvester.pCoresites.specNum * (age_largest / 5 + 1)) + (speciesOrder[k] - 1) * (age_largest / 5 + 1) + tempcount;
                        if (value <= tempend)
                        {
                            AgeArraySmall[pos] = value++;
                        }
                        else
                        {
                            AgeArraySmall[pos] = 0;
                        }
                    }
                    tempstart = AgeCohortGroup[(speciesOrder[k] - 1) * 6 + i_count + 1];
                    tempend = AgeCohortGroup[(speciesOrder[k] - 1) * 6 + i_count] + 1;
                    value = tempstart;
                    for (int tempcount = 0; tempcount < age_largest / 5 + 1; tempcount++)
                    {
                        int pos = i_count * (BoundedPocketStandHarvester.pCoresites.specNum * (age_largest / 5 + 1)) + (speciesOrder[k] - 1) * (age_largest / 5 + 1) + tempcount;
                        if (value >= tempend)
                        {
                            AgeArrayLarge[pos] = value--;
                        }
                        else
                        {
                            AgeArrayLarge[pos] = 0;
                        }
                    }
                }
            }
            if (Small0_Large1 == 0)
            {
                for (i_count = 0; i_count < 5; i_count++)
                {
                    for (int tempcount = 0; tempcount < age_largest / 5 + 1; tempcount++)
                    {
                        for (k = 0; k < BoundedPocketStandHarvester.pCoresites.specNum; k++)
                        {
                            int pos = i_count * (BoundedPocketStandHarvester.pCoresites.specNum * (age_largest / 5 + 1)) + (speciesOrder[k] - 1) * (age_largest / 5 + 1) + tempcount;
                            if (AgeArraySmall[pos] > 0)
                            {
                                m = AgeArraySmall[pos];
                                if (flag_cut[speciesOrder[k] - 1] == 1)
                                {
                                    flag_cut_anyspecie = 1;
                                    if (TmpStockingS < target)
                                    {

                                        singleTreeBA = BoundedPocketStandHarvester.pCoresites.GetGrowthRates(speciesOrder[k],m,l.ltID) * BoundedPocketStandHarvester.pCoresites.GetGrowthRates(speciesOrder[k],m,l.ltID) / 4 * 3.1415926 / 10000;
                                        tempBA = singleTreeBA * BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).getTreeNum(m, speciesOrder[k]);
                                        tempStocking = GetStockinginACell_spec_age(i, j, speciesOrder[k], m);

                                        if (tempStocking <= target - TmpStockingS)
                                        {
                                            treeNum_original = BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).getTreeNum(m, speciesOrder[k]);

                                            if (BoundedPocketStandHarvester.pCoresites[i, j].specAtt(speciesOrder[k]).MinSproutAge <= m * BoundedPocketStandHarvester.pCoresites.TimeStep && BoundedPocketStandHarvester.pCoresites[i, j].specAtt(speciesOrder[k]).maxSproutAge >= m * BoundedPocketStandHarvester.pCoresites.TimeStep)
                                            {
                                                BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).TreesFromVeg += treeNum_original;
                                            }
                                            BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).setTreeNum(m, speciesOrder[k], 0);
                                            TmpStockingS += tempStocking;
                                            BoundedPocketStandHarvester.pHarvestsites.AddMoreValueHarvestBA_spec(i, j, speciesOrder[k] - 1, tempBA);
                                            BoundedPocketStandHarvester.pHarvestsites.AddMoreValueHarvestBA(i, j, tempBA);
                                        }
                                        else
                                        {
                                            treeNum_save = (int) (BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).getTreeNum(m, speciesOrder[k]) * (1 - (target - TmpStockingS) / tempStocking));
                                            if (treeNum_save > 0)
                                            {
                                                treeNum_original = BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).getTreeNum(m, speciesOrder[k]);
                                                if (BoundedPocketStandHarvester.pCoresites[i, j].specAtt(speciesOrder[k]).MinSproutAge <= m * BoundedPocketStandHarvester.pCoresites.TimeStep && BoundedPocketStandHarvester.pCoresites[i, j].specAtt(speciesOrder[k]).maxSproutAge >= m * BoundedPocketStandHarvester.pCoresites.TimeStep)
                                                {
                                                    BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).TreesFromVeg += treeNum_original - treeNum_save;
                                                }
                                                BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).setTreeNum(m, speciesOrder[k], treeNum_save);
                                                TmpStockingS += (target - TmpStockingS);

                                                tempBA = tempBA - (treeNum_save * singleTreeBA);
                                                BoundedPocketStandHarvester.pHarvestsites.AddMoreValueHarvestBA_spec(i, j, speciesOrder[k] - 1, tempBA);

                                                BoundedPocketStandHarvester.pHarvestsites.AddMoreValueHarvestBA(i, j, tempBA);

                                            }
                                            else
                                            {
                                                treeNum_original = BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).getTreeNum(m, speciesOrder[k]);
                                                if (BoundedPocketStandHarvester.pCoresites[i, j].specAtt(speciesOrder[k]).MinSproutAge <= m * BoundedPocketStandHarvester.pCoresites.TimeStep && BoundedPocketStandHarvester.pCoresites[i, j].specAtt(speciesOrder[k]).maxSproutAge >= m * BoundedPocketStandHarvester.pCoresites.TimeStep)
                                                {
                                                    BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).TreesFromVeg += treeNum_original - treeNum_save;
                                                }
                                                BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).setTreeNum(m, speciesOrder[k], 0);
                                                TmpStockingS += tempStocking;

                                                tempBA = tempBA - (treeNum_save * singleTreeBA);
                                                BoundedPocketStandHarvester.pHarvestsites.AddMoreValueHarvestBA_spec(i, j, speciesOrder[k] - 1, tempBA);

                                                BoundedPocketStandHarvester.pHarvestsites.AddMoreValueHarvestBA(i, j, tempBA);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (k = 0; k < BoundedPocketStandHarvester.pCoresites.specNum; k++)
                                        {
                                            if (flag_plant[speciesOrder[k] - 1] == 1)
                                            {
                                                int tree_left = BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).getTreeNum(1, speciesOrder[k]);
                                                BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).setTreeNum(1, speciesOrder[k], num_TreePlant[speciesOrder[k] - 1] + tree_left);
                                            }
                                        }
                                        AgeCohortGroup = null;
                                        AgeArraySmall = null;
                                        AgeArrayLarge = null;
                                        return TmpStockingS;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (Small0_Large1 == 1)
            {
                for (i_count = 4; i_count >= 0; i_count--)
                {
                    for (int tempcount = 0; tempcount < age_largest / 5 + 1; tempcount++)
                    {
                        for (k = 0; k < BoundedPocketStandHarvester.pCoresites.specNum; k++)
                        {
                            int pos = i_count * (BoundedPocketStandHarvester.pCoresites.specNum * (age_largest / 5 + 1)) + (speciesOrder[k] - 1) * (age_largest / 5 + 1) + tempcount;
                            if (AgeArrayLarge[pos] > 0)
                            {
                                m = AgeArrayLarge[pos];
                                if (flag_cut[speciesOrder[k] - 1] == 1)
                                {
                                    flag_cut_anyspecie = 1;
                                    if (TmpStockingS < target)
                                    {
                                        singleTreeBA = BoundedPocketStandHarvester.pCoresites.GetGrowthRates(speciesOrder[k], m, l.ltID) * BoundedPocketStandHarvester.pCoresites.GetGrowthRates(speciesOrder[k], m, l.ltID) / 4 * 3.1415926 / 10000;
                                        tempBA = singleTreeBA * BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).getTreeNum(m, speciesOrder[k]);

                                        tempStocking = GetStockinginACell_spec_age(i, j, speciesOrder[k], m);
                                        if (tempStocking <= target - TmpStockingS)
                                        {
                                            treeNum_original = BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).getTreeNum(m, speciesOrder[k]);
                                            if (BoundedPocketStandHarvester.pCoresites[i, j].specAtt(speciesOrder[k]).MinSproutAge <= m * BoundedPocketStandHarvester.pCoresites.TimeStep && BoundedPocketStandHarvester.pCoresites[i, j].specAtt(speciesOrder[k]).maxSproutAge >= m * BoundedPocketStandHarvester.pCoresites.TimeStep)
                                            {
                                                BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).TreesFromVeg += treeNum_original;
                                            }
                                            BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).setTreeNum(m, speciesOrder[k], 0);
                                            TmpStockingS += tempStocking;

                                            BoundedPocketStandHarvester.pHarvestsites.AddMoreValueHarvestBA_spec(i, j, speciesOrder[k] - 1, tempBA);

                                            BoundedPocketStandHarvester.pHarvestsites.AddMoreValueHarvestBA(i, j, tempBA);

                                        }
                                        else
                                        {
                                            treeNum_save = (int) (BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).getTreeNum(m, speciesOrder[k]) * (1 - (target - TmpStockingS) / tempStocking));                                       
                                            if (treeNum_save > 0)
                                            {
                                                treeNum_original = BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).getTreeNum(m, speciesOrder[k]);
                                                if (BoundedPocketStandHarvester.pCoresites[i, j].specAtt(speciesOrder[k]).MinSproutAge <= m * BoundedPocketStandHarvester.pCoresites.TimeStep && BoundedPocketStandHarvester.pCoresites[i, j].specAtt(speciesOrder[k]).maxSproutAge >= m * BoundedPocketStandHarvester.pCoresites.TimeStep)
                                                {
                                                    BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).TreesFromVeg += treeNum_original - treeNum_save;
                                                }
                                                BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).setTreeNum(m, speciesOrder[k], treeNum_save);
                                                TmpStockingS += (target - TmpStockingS);

                                                tempBA = tempBA - (treeNum_save * singleTreeBA);
                                                BoundedPocketStandHarvester.pHarvestsites.AddMoreValueHarvestBA_spec(i, j, speciesOrder[k] - 1, tempBA);

                                                BoundedPocketStandHarvester.pHarvestsites.AddMoreValueHarvestBA(i, j, tempBA);

                                            }
                                            else
                                            {
                                                treeNum_original = BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).getTreeNum(m, speciesOrder[k]);
                                                if (BoundedPocketStandHarvester.pCoresites[i, j].specAtt(speciesOrder[k]).MinSproutAge <= m * BoundedPocketStandHarvester.pCoresites.TimeStep && BoundedPocketStandHarvester.pCoresites[i, j].specAtt(speciesOrder[k]).maxSproutAge >= m * BoundedPocketStandHarvester.pCoresites.TimeStep)
                                                {
                                                    BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).TreesFromVeg += treeNum_original - treeNum_save;
                                                }
                                                BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).setTreeNum(m, speciesOrder[k], 0);
                                                TmpStockingS += tempStocking;

                                                tempBA = tempBA - (treeNum_save * singleTreeBA);
                                                BoundedPocketStandHarvester.pHarvestsites.AddMoreValueHarvestBA_spec(i, j, speciesOrder[k] - 1, tempBA);

                                                BoundedPocketStandHarvester.pHarvestsites.AddMoreValueHarvestBA(i, j, tempBA);

                                            }
                                        }
                                    }
                                    else
                                    {
                                        for (k = 0; k < BoundedPocketStandHarvester.pCoresites.specNum; k++)
                                        {
                                            if (flag_plant[speciesOrder[k] - 1] == 1)
                                            {
                                                int tree_left = BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).getTreeNum(1, speciesOrder[k]);
                                                BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).setTreeNum(1, speciesOrder[k], num_TreePlant[speciesOrder[k] - 1] + tree_left);
                                            }
                                        }
                                        AgeCohortGroup = null;
                                        AgeArraySmall = null;
                                        AgeArrayLarge = null;
                                        return TmpStockingS;
                                    }
                                }
                            }
                        }
                    }
                }         
            }
            if (flag_cut_anyspecie != 0)
            {

                for (k = 0; k < BoundedPocketStandHarvester.pCoresites.specNum; k++)
                {

                    if (flag_plant[speciesOrder[k] - 1] == 1)
                    {
                        int tree_left = BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).getTreeNum(1, speciesOrder[k]);
                        BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(speciesOrder[k]).setTreeNum(1, speciesOrder[k], num_TreePlant[speciesOrder[k] - 1] + tree_left);
                    }
                }
            }
            AgeCohortGroup = null;
            AgeArraySmall = null;
            AgeArrayLarge = null;
            return TmpStockingS;
        }


        public double GetStockinginACell(int i, int j)
        {

            int m;
            int k;
            double num_trees = 0; //N
            double Diameters = 0; //D
            double Diameters_square = 0; //D^2
            double x = BoundedPocketStandHarvester.pCoresites.stocking_x_value;
            double y = BoundedPocketStandHarvester.pCoresites.stocking_y_value;
            double z = BoundedPocketStandHarvester.pCoresites.stocking_z_value;
            Landunit l;
            l = BoundedPocketStandHarvester.pCoresites.locateLanduPt(i, j);
            for (k = 1; k <= BoundedPocketStandHarvester.pCoresites.specNum; k++)
            {

                for (m = 1; m <= BoundedPocketStandHarvester.pCoresites[i, j].specAtt(k).longevity / BoundedPocketStandHarvester.pCoresites.TimeStep; m++)
                {
                    num_trees += BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(k).getTreeNum(m, k);
                    Diameters += BoundedPocketStandHarvester.pCoresites.GetGrowthRates(k, m, l.ltID) * BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(k).getTreeNum(m, k) / 2.54;
                    Diameters_square += BoundedPocketStandHarvester.pCoresites.GetGrowthRates(k, m, l.ltID) * BoundedPocketStandHarvester.pCoresites.GetGrowthRates(k, m, l.ltID) * BoundedPocketStandHarvester.pCoresites[i, j].SpecieIndex(k).getTreeNum(m, k) / 2.54 / 2.54;
                }
            }
            return (x * num_trees + y * Diameters + z * Diameters_square) / (BoundedPocketStandHarvester.pCoresites.CellSize * BoundedPocketStandHarvester.pCoresites.CellSize / 4046.86);
        }

        public double computeStandStocking(Stand stand)
        {
            Ldpoint p = new Ldpoint();
            Site site;
            double count = 0;
            int m;
            int k;
            double num_trees = 0; //N
            double Diameters = 0; //D
            double Diameters_square = 0; //D^2
            double x = BoundedPocketStandHarvester.pCoresites.stocking_x_value;
            double y = BoundedPocketStandHarvester.pCoresites.stocking_y_value;
            double z = BoundedPocketStandHarvester.pCoresites.stocking_z_value;
            Landunit l;
            for (StandIterator it = new StandIterator(stand); it.moreSites(); it.gotoNextSite())
            {
                p = it.getCurrentSite();
                site = BoundedPocketStandHarvester.pCoresites[p.y, p.x];
                l = BoundedPocketStandHarvester.pCoresites.locateLanduPt(p.y, p.x);
                count += 1;
                for (k = 1; k <= BoundedPocketStandHarvester.pCoresites.specNum; k++)
                {

                    for (m = 1; m <= site.specAtt(k).longevity / BoundedPocketStandHarvester.pCoresites.TimeStep; m++)
                    {
                        num_trees += site.SpecieIndex(k).getTreeNum(m, k);
                        Diameters += BoundedPocketStandHarvester.pCoresites.GetGrowthRates(k, m, l.ltID) * site.SpecieIndex(k).getTreeNum(m, k) / 2.54;
                        Diameters_square += BoundedPocketStandHarvester.pCoresites.GetGrowthRates(k, m, l.ltID) * BoundedPocketStandHarvester.pCoresites.GetGrowthRates(k, m, l.ltID) * site.SpecieIndex(k).getTreeNum(m, k) / 2.54 / 2.54;
                    }
                }
            }
            return (x * num_trees + y * Diameters + z * Diameters_square) / (BoundedPocketStandHarvester.pCoresites.CellSize * BoundedPocketStandHarvester.pCoresites.CellSize / 4046.86) / stand.numberOfActiveSites();
        }

        public override void Read(StreamReader infile)
        {
            if (itsState == Enum.START)
            {
                base.Read(infile);
                itsState = Enum.PENDING;
            }
            else
                throw new Exception("Illegal call to HarvestVolumeFittingRegime::read.");
        }

        public override void readCustomization1(StreamReader infile)
        {
            string instring;
            string[] sarray;

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading entry decade from harvest section.");
            sarray = instring.Split('#');
            itsEntryDecade = int.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading reentry from harvest section.");
            sarray = instring.Split('#');
            itsRepeatInterval = int.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading TargetVolume from harvest section.");
            sarray = instring.Split('#');
            Mininum_Stocking = double.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading TargetVolume from harvest section.");
            sarray = instring.Split('#');
            Small0_Large1 = int.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading target proportion from harvest section.");
            sarray = instring.Split('#');
            targetProportion = double.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading TargetVolume from harvest section.");
            sarray = instring.Split('#');
            TargetStocking = double.Parse(sarray[0]);

            itsTargetCut = (int)(BoundedPocketStandHarvester.managementAreas[getManagementAreaId()].numberofActiveSites() * targetProportion);

            if (BoundedPocketStandHarvester.pCoresites.specNum > 200)
                throw new Exception("Two many species for harvest.");

            instring = infile.ReadLine();
            instring = infile.ReadLine();
            instring = infile.ReadLine();
            for (int i = 0; i < BoundedPocketStandHarvester.pCoresites.specNum; i++)
            {
                int temp_spec_order;
                instring = infile.ReadLine();
                sarray = instring.Split(' ');

                temp_spec_order = int.Parse(sarray[0]);
                speciesOrder[temp_spec_order - 1] = i + 1;
                flag_cut[i] = int.Parse(sarray[1]);
                flag_plant[i] = int.Parse(sarray[2]);
                num_TreePlant[i] = int.Parse(sarray[3]);
            }

            copy_initial_parameters();

            total_reentry_event_instances = 0;
            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading standard deviation from harvest section.");
            sarray = instring.Split('#');
            total_reentry_event_instances = int.Parse(sarray[0]);

            instring = infile.ReadLine();

            if (total_reentry_event_instances > 0)
            {
                StockingCuttingRegime_reentry_event_instances = new StockingCuttingRegime_reentry_event[total_reentry_event_instances];
            }
            for (int ii = 0; ii < total_reentry_event_instances; ii++)
            {
                StockingCuttingRegime_reentry_event_instances[ii] = new StockingCuttingRegime_reentry_event();
                StockingCuttingRegime_reentry_event_instances[ii].StockingCuttingRegime_load_reentry_parameters(infile);
            }
            itsTargetCut = (int)(BoundedPocketStandHarvester.managementAreas[getManagementAreaId()].numberofActiveSites() * targetProportion);
            StandsCut = 0;
            SitesCut = 0;
        }

        public override void readCustomization2(StreamReader infile)
        {
            setDuration(1);
        }

        public void send_parameters_to_current(int flag, int index)
        {
            if (flag == 1)
            { // initial or repeat
                itsTargetCut = itsTargetCut_copy;
                targetProportion = targetProportion_copy;
                TargetStocking = TargetStocking_copy;
                Mininum_Stocking = Mininum_Stocking_copy;
                Small0_Large1 = Small0_Large1_copy;
                for (int i = 0; i < 200; i++)
                {
                    speciesOrder[i] = speciesOrder_copy[i];
                    flag_plant[i] = flag_plant_copy[i];
                    flag_cut[i] = flag_cut_copy[i];
                    num_TreePlant[i] = num_TreePlant_copy[i];
                }
                itsTargetCut = (int)((float)(BoundedPocketStandHarvester.managementAreas[getManagementAreaId()].numberofActiveSites()) * targetProportion);
                StandsCut = 0;
                SitesCut = 0;
            }
            else if (flag == 0 && index < total_reentry_event_instances)
            { //re-entry
                itsTargetCut = StockingCuttingRegime_reentry_event_instances[index].itsTargetCut;
                targetProportion = StockingCuttingRegime_reentry_event_instances[index].targetProportion;
                TargetStocking = StockingCuttingRegime_reentry_event_instances[index].TargetStocking;
                Mininum_Stocking = StockingCuttingRegime_reentry_event_instances[index].Mininum_Stocking;
                Small0_Large1 = StockingCuttingRegime_reentry_event_instances[index].Small0_Large1;
                for (int i = 0; i < 200; i++)
                {
                    speciesOrder[i] = StockingCuttingRegime_reentry_event_instances[index].speciesOrder[i];
                    flag_plant[i] = StockingCuttingRegime_reentry_event_instances[index].flag_plant[i];
                    flag_cut[i] = StockingCuttingRegime_reentry_event_instances[index].flag_cut[i];
                    num_TreePlant[i] = StockingCuttingRegime_reentry_event_instances[index].num_TreePlant[i];
                }
                itsTargetCut = (int)((float)(BoundedPocketStandHarvester.managementAreas[getManagementAreaId()].numberofActiveSites()) * targetProportion);
                StandsCut = 0;
                SitesCut = 0;
            }
        }


        public void copy_initial_parameters()
        {
            if (1 == 1)
            { // initial or repeat
                itsTargetCut_copy = itsTargetCut;
                targetProportion_copy = targetProportion;
                TargetStocking_copy = TargetStocking;
                Mininum_Stocking_copy = Mininum_Stocking;
                Small0_Large1_copy = Small0_Large1;
                for (int i = 0; i < 200; i++)
                {
                    speciesOrder_copy[i] = speciesOrder[i];
                    flag_plant_copy[i] = flag_plant[i];
                    flag_cut_copy[i] = flag_cut[i];
                    num_TreePlant_copy[i] = num_TreePlant[i];
                }
            }
        }

    }

    public class StockingCuttingRegime_reentry_event
    {
        public int itsReentryInteval;
        public int itsEntryDecade;
        public int itsFinalDecade;
        public int itsTargetCut;
        public double targetProportion;
        public double TargetStocking; //Aug 03 2009
        public double Mininum_Stocking; //May 26 2011
        public int Small0_Large1; //May 26 2011
        public int[] speciesOrder = new int[200]; //May 26 2011
        public int[] flag_plant = new int[200];
        public int[] flag_cut = new int[200];
        public int[] num_TreePlant = new int[200];

        public void StockingCuttingRegime_load_reentry_parameters(StreamReader infile)
        {
            string instring;
            string[] sarray;

            instring = infile.ReadLine();
            instring = infile.ReadLine();
            instring = infile.ReadLine();
            instring = infile.ReadLine();
            instring = infile.ReadLine();


            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading entry decade from harvest section.");
            sarray = instring.Split('#');
            itsReentryInteval = int.Parse(sarray[0]);

            int itsRepeatInterval;
            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading reentry from harvest section.");
            sarray = instring.Split('#');
            itsRepeatInterval = int.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading TargetVolume from harvest section.");
            sarray = instring.Split('#');
            Mininum_Stocking = double.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading TargetVolume from harvest section.");
            sarray = instring.Split('#');
            Small0_Large1 = int.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading target proportion from harvest section.");
            sarray = instring.Split('#');
            targetProportion = double.Parse(sarray[0]);

            if ((instring = infile.ReadLine()) == null)
                throw new Exception("Error reading TargetVolume from harvest section.");
            sarray = instring.Split('#');
            TargetStocking = double.Parse(sarray[0]);          

            if (BoundedPocketStandHarvester.pCoresites.specNum > 200)
                throw new Exception("Two many species for harvest.");

            instring = infile.ReadLine();
            instring = infile.ReadLine();
            instring = infile.ReadLine();
            for (int i = 0; i < BoundedPocketStandHarvester.pCoresites.specNum; i++)
            {
                int temp_spec_order;
                instring = infile.ReadLine();
                sarray = instring.Split(' ');

                temp_spec_order = int.Parse(sarray[0]);
                speciesOrder[temp_spec_order - 1] = i + 1;
                flag_cut[i] = int.Parse(sarray[1]);
                flag_plant[i] = int.Parse(sarray[2]);
                num_TreePlant[i] = int.Parse(sarray[3]);
            }
            instring = infile.ReadLine();
        }

    }
}
