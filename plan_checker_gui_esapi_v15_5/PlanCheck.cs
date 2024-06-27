using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualBasic.FileIO;
using System.Collections.Specialized;

using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

//namespace VMS.TPS
namespace plan_checker_gui_esapi_v15_5
{
    internal class PlanCheck
    {
        public class Error
        {
            public bool IsError { get; set; }
            public string Message { get; set; }

            public Error () {
                IsError = false;
                Message = "";
            }

            public Error(bool is_error, string message)
            {
                IsError = is_error;
                Message = message;
            }
        }

        public enum Technique 
        {
            Conventional,
            IMRT,
            Electron
        }

        public enum MlcId
        {
            Millennium80,
            Millennium120,
            HD120
        }

        public struct CheckSettings
        {
            public string mlc_model_millennium80;
            public string mlc_model_millennium120;
            public string mlc_model_hd120;
            public double mlc_jaw_dist_tole;
            public string photon_calc_algo_conv;
            public string photon_calc_algo_imrt;
            public string electron_calc_algo;
            public double photon_calc_grid_conv;
            public List<double> photon_calc_grid_imrt;
            public double electron_calc_grid;
            public Dictionary<(string, string), Int32> dose_rate_dict;
            public Dictionary<string, List<string>> linac_couch_dict;
        }

        public readonly struct MlcLeafDim
        {

            public Int32 MaxMlcX1Mm { get; }
            public Int32 MaxMlcX2Mm { get; }
            public Int32 MaxMlcY1Mm { get; }
            public Int32 MaxMlcY2Mm { get; }

            public Double InnerWidthMm { get; }
            public Double OuterWidthMm { get; }
            public UInt32 NumOfInner { get; }
            public UInt32 NumOfOuter { get; }

            private List<Double> EdgeListMm { get; }


            public MlcLeafDim(MlcId id)
            {
                EdgeListMm = new List<double>() { 0.0 };

                if (id == MlcId.Millennium80)
                {
                    MaxMlcX1Mm = 200;
                    MaxMlcX2Mm = 200;
                    MaxMlcY1Mm = 200;
                    MaxMlcY2Mm = 200;

                    InnerWidthMm = 0.0;
                    NumOfInner = 0;
                    OuterWidthMm = 10.0;
                    NumOfOuter = 40;

                    fill_edge_coordinates_of_mlc();
                }
                else if (id == MlcId.Millennium120)
                {
                    MaxMlcX1Mm = 200;
                    MaxMlcX2Mm = 200;
                    MaxMlcY1Mm = 200;
                    MaxMlcY2Mm = 200;

                    InnerWidthMm = 5.0;
                    NumOfInner = 40;
                    OuterWidthMm = 10.0;
                    NumOfOuter = 20;

                    fill_edge_coordinates_of_mlc();
                }
                else if (id == MlcId.HD120)
                {
                    MaxMlcX1Mm = 160;
                    MaxMlcX2Mm = 160;
                    MaxMlcY1Mm = 110;
                    MaxMlcY2Mm = 110;

                    InnerWidthMm = 2.5;
                    NumOfInner = 32;
                    OuterWidthMm = 5.0;
                    NumOfOuter = 28;

                    fill_edge_coordinates_of_mlc();
                }
                else
                {
                    MaxMlcX1Mm = 0;
                    MaxMlcX2Mm = 0;
                    MaxMlcY1Mm = 0;
                    MaxMlcY2Mm = 0;

                    InnerWidthMm = 0;
                    NumOfInner = 0;
                    OuterWidthMm = 0;
                    NumOfOuter = 0;

                    fill_edge_coordinates_of_mlc();
                }
            }


            public void fill_edge_coordinates_of_mlc()
            {
                for (int i = 0; i < NumOfOuter / 2; ++i)
                {
                    EdgeListMm.Add(EdgeListMm.LastOrDefault() + OuterWidthMm);
                }
                for (int i = 0; i < NumOfInner; ++i)
                {
                    EdgeListMm.Add(EdgeListMm.LastOrDefault() + InnerWidthMm);
                }
                for (int i = 0; i < NumOfOuter / 2; ++i)
                {
                    EdgeListMm.Add(EdgeListMm.LastOrDefault() + OuterWidthMm);
                }

                return;
            }

            public (Double, Double) get_y_coordinate_of_leaf(int i)
            {
                //            coord_y1 = EdgeListMm.ElementAt(i); 
                //            coord_y2 = EdgeListMm.ElementAt(i + 1);
                Double coord_y1 = EdgeListMm.ElementAt(i) - MaxMlcY1Mm;
                Double coord_y2 = EdgeListMm.ElementAt(i + 1) - MaxMlcY1Mm;

                return (coord_y1, coord_y2);
            }

        }


        public class CheckedPlan
        {
            public CheckSettings Settings { get; }
//            public string CourceID { get; }
            public string PlanID { get; }
            public string PatientID { get; }
            public string PatientName { get; }

            public int NumOfFraction { get; }
            public VMS.TPS.Common.Model.Types.DoseValue DosePerFraction { get; }
            public VMS.TPS.Common.Model.Types.DoseValue TotalDose { get; }
            public string MachineId { get; }
            public List<string> Energies { get; }


            public string PhotonCalcAlgorithm { get; }
            public string ElecCalcAlgorithm { get; }
//            public string NormalizationMethod { get; }
//            public VMS.TPS.Common.Model.Types.VVector NormalizationPoint { get; }
//
//            public List<Int32> DoseRates { get; }
//            public List<VMS.TPS.Common.Model.Types.VVector> IsoCenters { get; }
//            public VMS.TPS.Common.Model.API.ReferencePoint PrimaryReferencePoint { get; }
//
//            public List<double> Arclengths { get; }
            
            public Technique IrradiationTechnique { get; }



            public Error IsCouchOk { get; }

            //        public bool IsJawError { get; }
            public Error IsJawMlcOk { get; }
            //        public string LeafError { get; }

            public Error IsCalcAlgorithmOk { get; }
            public Error IsDoseRateOk { get; }
            public Error IsNormalizationMethodOk { get; }
            public Error IsIsocenterRefpointMatchedOk { get; }
            public Error IsIsocenterPrecisionOk { get; }
            public Error IsMatchedWithDrPlanOk { get; }
            public Error IsGantryAnglePrecisionOk { get; }
            public Error IsMuOk { get; }
            public Error IsJawApertureOk { get; }


            public CheckedPlan(in ScriptContext context, in string file_path)
            {
                /* Load settings */
                Settings = LoadSettings(file_path);

                /* Plan check */
                PlanID = context.ExternalPlanSetup.Id;
                PatientID = "(" + context.Patient.Id + ", " + context.Patient.Id2 + ")";
                //                PatientName = context.Patient.Name;
                PatientName = context.Patient.LastName + " " + context.Patient.FirstName;

                NumOfFraction = context.ExternalPlanSetup.NumberOfFractions ?? 0;
                DosePerFraction = context.ExternalPlanSetup.DosePerFraction;
                TotalDose = context.ExternalPlanSetup.TotalDose;

                MachineId = context.ExternalPlanSetup.Beams.ElementAt(0).TreatmentUnit.Id;
                Energies = new();
                foreach (Beam beam in context.ExternalPlanSetup.Beams)
                {
                    var ene = beam.EnergyModeDisplayName;
                    if (!Energies.Contains(ene))
                    {
                        Energies.Add(ene);
                    }
                    else { }
                }


                PhotonCalcAlgorithm = context.ExternalPlanSetup.PhotonCalculationModel;
                ElecCalcAlgorithm = context.ExternalPlanSetup.ElectronCalculationModel;
//                foreach (var foo in context.ExternalPlanSetup.PhotonCalculationOptions)
//                {
//                    string message = foo.Key + ": " + foo.Value;
//                    MessageBox.Show(message);
//                }
//                foreach (var bar in context.ExternalPlanSetup.ElectronCalculationOptions)
//                {
//                    string message = bar.Key + ": " + bar.Value;
//                    MessageBox.Show(message);
//                }

                IrradiationTechnique = Technique.Conventional;
                foreach (Beam beam in context.ExternalPlanSetup.Beams)
                {
                    // Enum: MLCPlanType 
                    // name              Value   Description
                    // Static                0   The MLC shape is static during the beam - on.
                    // DoseDynamic           1   The MLC shape and dose per degree are changed dynamically during the beam - on.The gantry does not rotate.  
                    // ArcDynamic            2   The MLC shape is changed dynamically during the beam - on.The dose per degree is kept constant. The gantry rotates.
                    // VMAT                  3   The MLC shape and dose per degree are changed dynamically during the beam - on.This MLC type is used in RapidArc and other VMAT fields.
                    // ProtonLayerStacking   4   Proton layer stacking.
                    // NotDefined          999   Undefined MLC Plan type. 

                    // for Hybrid irradiation
                    if (IrradiationTechnique == Technique.IMRT)
                    {
                        IrradiationTechnique = Technique.IMRT;
                    }
                    else if (beam.GetOptimalFluence() != null)  // sliding window IMRT
                    {
                        IrradiationTechnique = Technique.IMRT;
                    }
                    else if ((beam.MLC != null) && 
                        ( (beam.MLCPlanType == MLCPlanType.VMAT) || ((context.ExternalPlanSetup.DosePerFraction.Dose >= 700) && (context.ExternalPlanSetup.NumberOfFractions.Value > 1)) ) )
                    {
                        IrradiationTechnique = Technique.IMRT;
                    }
                    // electron or not
                    else if (beam.Applicator != null)
                    {
                        IrradiationTechnique = Technique.Electron;
                    }
                    else
                    {
                        IrradiationTechnique = Technique.Conventional;
                    }

//                    MessageBox.Show(string.Format("ID: {0}, Technique: {1}, ControlPoints: {2}", beam.Id, IrradiationTechnique.ToString(), beam.ControlPoints.Count));
                }


                // check
                IsCouchOk = IsVirtualCouchExist(context);
                IsJawMlcOk = JawMlcDistanceCheck(context);
                IsCalcAlgorithmOk = IsCalculationAlgorithmOk(context);
                IsDoseRateOk = IsDoseRateMax(context);
                IsNormalizationMethodOk = IsNormalizationMethodAppropriate(context);
                IsIsocenterRefpointMatchedOk = IsIsocentersRefpointMatched(context);
                IsIsocenterPrecisionOk = IsIsocenterPrecisionIntMM(context);
                IsGantryAnglePrecisionOk = IsGantryAngleInteger(context);
//                IsMatchedWithDrPlanOk = (context);
                IsMuOk = IsMuMoreThanOrEqualTo10(context);
                IsJawApertureOk = IsJawMoreThanOrEqualTo3(context);

            }

            public CheckSettings LoadSettings(in string file_path)
            {
                CheckSettings settings = new();
                settings.photon_calc_grid_imrt = new();
                settings.dose_rate_dict = new();
                settings.linac_couch_dict = new();

                try
                {
                    var parser = new TextFieldParser(file_path);
                    parser.Delimiters = new string[] { "," };
                    string foo = "";
                    while(!parser.EndOfData)
                    {
                        var fields = parser.ReadFields();

                        switch (fields[0])
                        {

                            case "MlcJawToleranceDistance":
                                settings.mlc_jaw_dist_tole = (fields.Length == 2) ? Double.Parse(fields[1]) : 0.0;
                                if (fields.Length != 2)
                                {
                                    foo += "MlcJawToleranceDistance has invalid filed.\n";
                                }
                                else { }
                                break;

                            case "PhotonCalcAlgorithmConv":
                                settings.photon_calc_algo_conv = (fields.Length == 2) ? fields[1] : "";
                                if (fields.Length != 2)
                                {
                                    foo += "PhotonCalcAlgorithmConv has invalid filed.\n";
                                }
                                else { }
                                break;

                            case "PhotonCalcAlgorithmImrtSrt":
                                settings.photon_calc_algo_imrt = (fields.Length == 2) ? fields[1] : "";
                                if (fields.Length != 2)
                                {
                                    foo += "PhotonCalcAlgorithmImrtSrt has invalid filed.\n";
                                }
                                else { }
                                break;

                            case "ElectronCalcAlgorithm":
                                settings.electron_calc_algo = (fields.Length == 2) ? fields[1] : "";
                                if (fields.Length != 2)
                                {
                                    foo += "ElectronCalcAlgorithm has invalid filed.\n";
                                }
                                else { }
                                break;

                            case "PhotonCalcGridConv":
                                settings.photon_calc_grid_conv = (fields.Length == 2) ? Double.Parse(fields[1]) : 0.0;
                                if (fields.Length != 2)
                                {
                                    foo += "PhotonCalcGridConv has invalid filed.\n";
                                }
                                else { }
                                break;

                            case "PhotonCalcGridImrtSrt":
                                if (fields.Length != 2)
                                {
                                    foo += "PhotonCalcGridImrtSrt has invalid filed.\n";
                                    break;
                                }
                                else
                                {
                                    settings.photon_calc_grid_imrt.Add(Double.Parse(fields[1]));
                                    break;
                                }

                            case "ElectronCalcGrid":
                                settings.electron_calc_grid = (fields.Length == 2) ? Double.Parse(fields[1]) : 0.0;
                                if (fields.Length != 2)
                                {
                                    foo += "ElectronCalcGrid has invalid filed.\n";
                                }
                                else { }
                                break;

                            case "DoseRate":
                                if (fields.Length != 4)
                                {
                                    foo += "DoseRate has invalid filed.\n";
                                    break;
                                }
                                else
                                {
                                    settings.dose_rate_dict.Add((fields[1], fields[2]), Int32.Parse(fields[3]));
                                    break;
                                }

                            case "LinacCouchPair":
                                if ((fields.Length != 3) && (fields.Length != 4))
                                {
                                    foo += "DoseRate has invalid filed.\n";
                                    break;
                                }
                                else
                                {
                                    if (fields.Length == 4)
                                    {
                                        fields[2] = string.Format("{0}, {1}", fields.ElementAt(2), fields.ElementAt(3));
                                    }
                                    else {; }

                                    if (settings.linac_couch_dict.ContainsKey(fields[1]) == false)
                                    {
                                        settings.linac_couch_dict.Add(fields[1], new List<string>());
                                    }
                                    else {; }
                                    settings.linac_couch_dict[fields[1]].Add(fields[2]);

                                    break;
                                }

                            default:
                                break;

                        }

                        //                    foreach(var filed in fields)
                        //                    {
                        //                        foo += " " + filed;
                        //                    }
                        //                    MessageBox.Show(foo);
                    }

                    if (foo != "")
                    {
                        MessageBox.Show(foo);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                return settings;
            }

            public Error IsVirtualCouchExist(in ScriptContext context)
            {
                Error res = new Error(true, "・ Virtual couch がありません。\n");

                if ((IrradiationTechnique == Technique.Conventional) || (IrradiationTechnique == Technique.Electron))
                {
                    //                    is_error = false;
                    //                    has_couch = "";
                    res.IsError = false;
                    res.Message = "";
                }
                // for IMRT
                else
                {
                    string machine_id = context.ExternalPlanSetup.Beams.ElementAt(0).TreatmentUnit.Id;

                    foreach (var structure in context.ExternalPlanSetup.StructureSet.Structures)
                    {
                        if (structure.Id.Contains("Couch"))
                        {
                            List<string> values;
                            var has_key = Settings.linac_couch_dict.TryGetValue(machine_id, out values);

                            if ((has_key == true) && (values.Contains(structure.Comment)))
                            {
                                res.IsError = false;
                                res.Message = string.Format("\tLinac: {0}, Couch: {1}\n", machine_id, structure.Comment);
                            }
                            else
                            {
                                res.IsError = true;
//                                res.Message = "・ Virtual couch and Linac is not matched.\n" +
                                res.Message = "・ Virtual couch が Linac と一致しません。\n" +
                                                string.Format("\tLinac: {0}, Couch: {1}\n", machine_id, structure.Comment);
                                break;
                            }
                        }
                        else {; }
                    }
                }


                return res;
            }

            public Error JawMlcDistanceCheck(in ScriptContext context)
            {
                // print Jaw and MLC apperture
                // tolerance distance between Jaw and MLC are [0, 2) cm
                bool is_error = false;
                string jaw_error = "";
//                const double TOLERANCE_DISTANCE = 2.0;
                double TOLERANCE_DISTANCE = Settings.mlc_jaw_dist_tole;
                foreach (Beam beam in context.ExternalPlanSetup.Beams)
                {

                    // Enum: MLCPlanType 
                    // name              Value   Description
                    // Static                0   The MLC shape is static during the beam - on.
                    // DoseDynamic           1   The MLC shape and dose per degree are changed dynamically during the beam - on.The gantry does not rotate.  
                    // ArcDynamic            2   The MLC shape is changed dynamically during the beam - on.The dose per degree is kept constant. The gantry rotates.
                    // VMAT                  3   The MLC shape and dose per degree are changed dynamically during the beam - on.This MLC type is used in RapidArc and other VMAT fields.
                    // ProtonLayerStacking   4   Proton layer stacking.
                    // NotDefined          999   Undefined MLC Plan type. 
                    if ((beam.MLC != null) && (beam.MLCPlanType == 0))
                    {
                        MlcLeafDim leaf_dim = (beam.MLC.Model == "Millennium 80") ? new MlcLeafDim(MlcId.Millennium80)
                                                : (beam.MLC.Model == "Millennium 120") ? new MlcLeafDim(MlcId.Millennium120)
                                                : (beam.MLC.Model == "Varian High Definition 120") ? new MlcLeafDim(MlcId.HD120)
                                                : new MlcLeafDim();

                        //                    // debug
                        //                    string jaw_boundary_string = "ID: " + beam.MLC.Id + "\n"
                        //                                    + "Name: " + beam.MLC.Name + "\n"
                        //                                    + "Model: " + beam.MLC.Model + "\n";
                        //                    jaw_boundary_string += "Inner Width: " + leaf_dim.InnerWidthMm + "\n"
                        //                                    + "Num of Inner: " + leaf_dim.NumOfInner + "\n"
                        //                                    + "Outer Width: " + leaf_dim.OuterWidthMm + "\n"
                        //                                    + "Num of Outer: " + leaf_dim.NumOfOuter + "\n";
                        //
                        //                    for (int i = 0; i < beam.ControlPoints.ElementAt(0).LeafPositions.GetLength(1); ++i)
                        //                    {
                        //                        jaw_boundary_string += "edge #" + (i + 1) 
                        //                            + ": (" + leaf_dim.get_y_coordinate_of_leaf(i).Item1 + ", " 
                        //                            + leaf_dim.get_y_coordinate_of_leaf(i).Item2 + ")" + "mm\n";
                        //                    }
                        //                    MessageBox.Show(jaw_boundary_string);
                        //                    // debug end

                        var jaw_pos = beam.ControlPoints.ElementAt(0).JawPositions;     // rect is represented by X1, X2, Y1, Y2
                                                                                        //                    var jaw_x1_pos = jaw_pos.X1;

//                        string foo = "X1: " + jaw_pos.X1 + "\n"
//                                        + "X2: " + jaw_pos.X2 + "\n"
//                                        + "Y1: " + jaw_pos.Y1 + "\n"
//                                        + "Y2: " + jaw_pos.Y2 + "\n";
//                        MessageBox.Show(foo);


                        var leaf_pos = beam.ControlPoints.ElementAt(0).LeafPositions;   // [0][n]: n-leaf of X-Jaw, [1][n]: n-leaf of Y-Jaw
                        var num_of_leaf_pairs = leaf_pos.GetLength(1);
                        var mlc_x1_min = 10000.0;
                        var mlc_x2_max = 0.0;

                        for (int i = 0; i < num_of_leaf_pairs; ++i)
                        {
                            // if Y-coordinates of the leaf is inside Jaw-difined field
                            if ((jaw_pos.Y1 < leaf_dim.get_y_coordinate_of_leaf(i).Item2) && (leaf_dim.get_y_coordinate_of_leaf(i).Item1 < jaw_pos.Y2))
                            {
                                // widest MLC position inside a Jaw-difined filed
                                mlc_x1_min = Math.Min(mlc_x1_min, leaf_pos[0, i]);
                                mlc_x2_max = Math.Max(mlc_x2_max, leaf_pos[1, i]);

                                // fully closed leaf-pair inside Jaw-difined filed
                                if ((jaw_pos.Y1 < leaf_dim.get_y_coordinate_of_leaf(i).Item1) && (leaf_dim.get_y_coordinate_of_leaf(i).Item2 < jaw_pos.Y2)
                                    && (leaf_pos[0, i] == leaf_pos[1, i]))
                                {
//                                    jaw_error += "\t" + beam.Id + " Leaf #" + (i + 1) + " is fully closed at X = " + leaf_pos[0, i] + ".\n";
                                    jaw_error += "\t" + beam.Id + " Leaf #" + (i + 1) + " が閉じています (X = " + leaf_pos[0, i] + ")。\n";
                                }
                                else {; }

                            }
                            else { continue; }
                        }

                        // MLC-defined field is smaller than the Jaw-defined field over the tolerance
//                        var jaw_mlc_diff_x1 = Math.Abs(jaw_pos.X1) - Math.Abs(mlc_x1_min);
//                        var jaw_mlc_diff_x2 = Math.Abs(jaw_pos.X2) - Math.Abs(mlc_x2_max);
                        var jaw_mlc_diff_x1 = mlc_x1_min - jaw_pos.X1;
                        var jaw_mlc_diff_x2 = jaw_pos.X2 - mlc_x2_max;

                        if (jaw_mlc_diff_x1 >= TOLERANCE_DISTANCE)
                        {
//                            jaw_error += "\t" + beam.Id + " Jaw X1 could be closed " + Math.Floor(jaw_mlc_diff_x1) + " mm more.\n";
                            jaw_error += "\t" + beam.Id + " Jaw X1 開度をあと " + Math.Floor(jaw_mlc_diff_x1) + " mm 狭くできます。\n";
                        }
                        else {; }

                        if (jaw_mlc_diff_x2 >= TOLERANCE_DISTANCE)
                        {
//                            jaw_error += "\t" + beam.Id + " Jaw X2 could be closed " + Math.Floor(jaw_mlc_diff_x2) + " mm more.\n";
                            jaw_error += "\t" + beam.Id + " Jaw X2 開度をあと " + Math.Floor(jaw_mlc_diff_x2) + " mm 狭くできます。\n";
                        }
                        else {; }

                    }

                }

                if (jaw_error != "")
                {
                    is_error = true;
//                    jaw_error = "Jaw and MLC positions could be modified.\n" + jaw_error;
                    jaw_error = "・ Jaw と MLC の位置が不適切な可能性があります。\n" + jaw_error;
                }
                else { }

                return new Error(is_error, jaw_error);
            }

            public Error IsCalculationAlgorithmOk(in ScriptContext context)
            {
                bool calc_ok = false;
                string calc_str = "";

                double calculation_grid_size = 0.0;

                // Calculation Algorithm and Grid size
                if (IrradiationTechnique == Technique.IMRT)
                {
                    calculation_grid_size = Convert.ToDouble(context.ExternalPlanSetup.PhotonCalculationOptions["CalculationGridSizeInCM"]);

                    bool grid_size_ok = false;
                    foreach (var grid_size in Settings.photon_calc_grid_imrt)
                    {
                        if (calculation_grid_size.Equals(grid_size))
                        {
                            grid_size_ok = true;
                            break;
                        }
                        else { }
                    }

                    if ((PhotonCalcAlgorithm == Settings.photon_calc_algo_imrt) && (grid_size_ok == true))
                    {
                        calc_ok = true;
                    }
                    else
                    {
                        calc_ok = false;
                    }
                }
                else if (IrradiationTechnique == Technique.Electron)
                {
                    calculation_grid_size = Convert.ToDouble(context.ExternalPlanSetup.ElectronCalculationOptions["CalculationGridSizeInCM"]);

                    if ((ElecCalcAlgorithm == Settings.electron_calc_algo)
                            && calculation_grid_size.Equals(Settings.electron_calc_grid))
                    {
                        calc_ok = true;
                    }
                    else
                    {
                        calc_ok = false;
                    }
                }
                else if (IrradiationTechnique == Technique.Conventional)
                {
                    calculation_grid_size = Convert.ToDouble(context.ExternalPlanSetup.PhotonCalculationOptions["CalculationGridSizeInCM"]);

                    if ((PhotonCalcAlgorithm == Settings.photon_calc_algo_conv)
                            && calculation_grid_size.Equals(Settings.photon_calc_grid_conv))
                    {
                        calc_ok = true;
                    }
                    else
                    {
                        calc_ok = false;
                    }
                }
                else { }


                string algorithm = (IrradiationTechnique == Technique.Electron) ? ElecCalcAlgorithm : PhotonCalcAlgorithm;

                if (!calc_ok)
                {
//                    calc_str = "・ The settings for dose calculation might be incorrect.\n"
//                                    + "\tMachine: " + MachineId
//                                    + "\tAlgorithm: " + algorithm 
//                                    + "\tGrid size: " + calculation_grid_size + "\n";
                    calc_str = "・ 線量計算の設定に誤りがある可能性があります。\n"
                                    + "\t照射装置: " + MachineId
                                    + "\t計算アルゴリズム: " + algorithm 
                                    + "\tグリッドサイズ: " + calculation_grid_size + "\n";
                }
                else
                {
//                    calc_str = "\tMachine: " + MachineId
//                                + "\tAlgorithm: " + algorithm 
//                                + "\tGrid size: " + calculation_grid_size + "\n";
                    calc_str = "\t照射装置: " + MachineId
                                + "\t計算アルゴリズム: " + algorithm 
                                + "\tグリッドサイズ: " + calculation_grid_size + "\n";

                }

                return new Error(!calc_ok, calc_str);
            }

            public Error IsDoseRateMax(in ScriptContext context)
            {
                bool is_error = false;
                string dr = "";

                foreach (Beam beam in context.ExternalPlanSetup.Beams)
                {

                    string energy = beam.EnergyModeDisplayName;
                    string machine_id = beam.TreatmentUnit.Id;
                    if (Settings.dose_rate_dict[(machine_id, energy)] != beam.DoseRate)
                    {
//                        dr_ok += beam.Id + "\t Dose rate: " + beam.DoseRate + "\n";
                        dr += "\t" + beam.Id + " 線量率: " + beam.DoseRate + "\n";
                    }
                    else { }
                }

                if (dr != "")
                {
                    is_error = true;
//                    dr = "・ Dose rate could be higher.\n" + dr;
                    dr = "・ 線量率が使用エネルギーでの最大値ではありません。\n" + dr;
                }
                else { }

                return new Error(is_error, dr);
            }
            
            public Error IsNormalizationMethodAppropriate(in ScriptContext context)
            {
                bool is_error = false;
                string norm = context.ExternalPlanSetup.PlanNormalizationMethod;
                string norm_ok = string.Format("\t{0}\n", norm);

                // for IMRT is not implemented
                if (IrradiationTechnique == Technique.IMRT)
                {
                    is_error = false;
                }
                // for Electron is not implemented
                else if (IrradiationTechnique == Technique.Electron) 
                { 
                    is_error = false;
                }
                else if ((norm.Contains("100% in Reference Point") == false)
                        && (norm.Contains("100% in Primary Reference Point") == false)
                        && (norm.Contains("100% in Isocenter of Field") == false)
                    )
                {
                    is_error = true;
                    norm_ok = "・ Normalizationが不適切な可能性があります。\n" + norm_ok;
                }
                else {
                    var dose_per_fraction = context.ExternalPlanSetup.DosePerFraction;
                    var planned_dose_per_fraction = context.ExternalPlanSetup.PlannedDosePerFraction;

                    if (dose_per_fraction.ValueAsString.Equals(planned_dose_per_fraction.ValueAsString))
                    {
                        is_error = false;
                    }
                    else
                    {
                        is_error = true;
//                        norm_ok = "Normalization method might not be appropriate.\n"
//                                    + String.Format("\tDosePerFraction: {0}, PlannedDosePerFraction: {1}\n", dose_per_fraction, planned_dose_per_fraction);
                        norm_ok = "・ Normalizationが不適切な可能性があります。\n"
                                    + String.Format("\tDosePerFraction: {0}, リファレンスポイントのDosePerFraction: {1}\n", dose_per_fraction, planned_dose_per_fraction);
                    }
                }

                return new Error (is_error, norm_ok);
            }

            public Error IsIsocentersRefpointMatched(in ScriptContext context)
            {
                bool is_error = false;
                string is_matched = "";
                var primary_ref_point = context.ExternalPlanSetup.PrimaryReferencePoint;

//                var scaling = 10e+6;
                var scaling = 10e+2;

                // for Conventional only
                if (IrradiationTechnique == Technique.Conventional)
                {
                    foreach (Beam beam in context.ExternalPlanSetup.Beams)
                    {
                        var isocenter_loc = beam.IsocenterPosition;
                        var refpoint_loc = primary_ref_point.GetReferencePointLocation(context.ExternalPlanSetup);

                        bool foo = (
                            ((Int32)Math.Round(refpoint_loc.x * scaling)).Equals((Int32)Math.Round(isocenter_loc.x * scaling))
                            && ((Int32)Math.Round(refpoint_loc.y * scaling)).Equals((Int32)Math.Round(isocenter_loc.y * scaling))
                            && ((Int32)Math.Round(refpoint_loc.z * scaling)).Equals((Int32)Math.Round(isocenter_loc.z * scaling))
                        );

                        if (foo) 
                        {
                            is_matched = "";
                        }
                        else
                        {
                            is_error = true;
//                            is_matched = "Isocenter and Reference position might be different.\n"
//                                    + String.Format("\tIsocenter point (x, y, z): ({0}, {1}, {2})\n",
//                                                        isocenter_loc.x / 10, isocenter_loc.y / 10, isocenter_loc.z / 10)
//                                                    + String.Format("\tRef. point (x, y, z): ({0}, {1}, {2})",
//                                                        refpoint_loc.x / 10, refpoint_loc.y / 10, refpoint_loc.z / 10) + "\n";
                            is_matched = "・ アイソセンタとリファレンスポイントの座標が異なります。\n"
                                    + String.Format("\tアイソセンタ座標 (x, y, z): ({0:f2}, {1:f2}, {2:f2})\n",
                                                        isocenter_loc.x / 10, isocenter_loc.y / 10, isocenter_loc.z / 10)
                                                    + String.Format("\tリファレンスポイント座標 (x, y, z): ({0:f2}, {1:f2}, {2:f2})",
                                                        refpoint_loc.x / 10, refpoint_loc.y / 10, refpoint_loc.z / 10) + "\n";
                        }
                    }
                }
                else {
                    is_matched = "";
                }

                return new Error(is_error, is_matched);
            }


            public Error IsIsocenterPrecisionIntMM(in ScriptContext context)
            {
                bool is_error = false;
                string is_ok = "";

                foreach (Beam beam in context.ExternalPlanSetup.Beams)
                {
                    var isocenter_loc = beam.IsocenterPosition;
                    var scaling = 10;

                    VVector loc_floor_frac = new( Math.Round(isocenter_loc.x * scaling), 
                                                            Math.Round(isocenter_loc.y * scaling), 
                                                            Math.Round(isocenter_loc.z * scaling));

                    if ((loc_floor_frac.x % scaling == 0) && (loc_floor_frac.y % scaling == 0) && (loc_floor_frac.z % scaling == 0))
                    {
                        is_ok = "";
                    }
                    else
                    {
                        is_error = true;
//                        is_ok = "Isocenter might have 2nd decimal place component.\n"
//                                    + String.Format("\tIsocenter point (x, y, z): ({0}, {1}, {2})",
//                                                    isocenter_loc.x / 10, isocenter_loc.y / 10, isocenter_loc.z / 10) + "\n";
                        is_ok = "・ アイソセンタ座標に小数点第二位以下の値があります。\n"
                                    + String.Format("\tアイソセンタ座標 (x, y, z): ({0:f2}, {1:f2}, {2:f2})",
                                                    isocenter_loc.x / 10, isocenter_loc.y / 10, isocenter_loc.z / 10) + "\n";
                    }
                }

                return new Error(is_error, is_ok);
            }

            public Error IsGantryAngleInteger(in ScriptContext context)
            {
                bool is_error = false;
                string is_ok = "";

                foreach (Beam beam in context.ExternalPlanSetup.Beams)
                {
                    var angle = beam.ControlPoints.First().GantryAngle;
//                    MessageBox.Show("angle: " + angle);
                    var scaling = 10;
                    var floor_angle = Math.Floor(angle * scaling);

                    if (floor_angle % scaling == 0)
                    {
//                        is_ok = "";
                    }
                    else
                    {
                        is_error = true;
//                        is_ok += beam.Id + "\tGantry angle of might be decimals.\n";
                        is_ok += "・ " + beam.Id + "\tガントリ角度が小数です。\n";
                    }
                }

                return new Error(is_error, is_ok);
            }

            public Error IsMuMoreThanOrEqualTo10(in ScriptContext context) { 
                bool is_error = false;
                string is_ok = "";

                foreach (Beam beam in context.ExternalPlanSetup.Beams)
                {
                    double minimum_mu = 10.0;
                    if (beam.Meterset.Value < minimum_mu)
                    {
                        is_ok += String.Format("\t{0} MU: {1}\n", beam.Id, String.Format("{0:f1}", beam.Meterset.Value));
                    }
                    else { }
                }

                if (is_ok!= "")
                {
                    is_error = true;
                    is_ok = "・ MUが10以下のFieldがあります。\n" + is_ok;
                }
                else { }

                return new Error(is_error, is_ok);
            }


            public Error IsJawMoreThanOrEqualTo3(in ScriptContext context) {
                Error err = new Error();

                foreach (Beam beam in context.ExternalPlanSetup.Beams)
                {
                    const double MINIMUM_APERTURE_MM = 30.0;
                    ControlPointCollection control_points_col = beam.ControlPoints;
                    IEnumerable<ControlPoint> control_points;
                    //                    MessageBox.Show(control_points.Count.ToString());

                    // Control points have a "start point" and a "stop point"
                    // If beam geometry is a static, not arc, start point and stop point indicate a same point.
                    // That the reson why control points have to be pop to prevent redundant analysis.
                    if (IrradiationTechnique == Technique.Conventional)
                    {
                        control_points = control_points_col.Take(1);
                    }
                    else {
                        control_points = control_points_col;
                    }

                    foreach (var cp in control_points)
                    {
                        var jaw_pos = cp.JawPositions;     // rect is represented by X1, X2, Y1, Y2 [mm]
                        var x_aperture = Math.Abs(jaw_pos.X1 - jaw_pos.X2);
                        var y_aperture = Math.Abs(jaw_pos.Y1 - jaw_pos.Y2);

//                        string foo = "X1: " + jaw_pos.X1 + "\n"
//                                        + "X2: " + jaw_pos.X2 + "\n"
//                                        + "Y1: " + jaw_pos.Y1 + "\n"
//                                        + "Y2: " + jaw_pos.Y2 + "\n";
//                        MessageBox.Show(foo);
//                        err.Message += String.Format("\t{0} Gantry: {1} [deg]\n", beam.Id, cp.GantryAngle);

                        if (x_aperture < MINIMUM_APERTURE_MM)
                        {
                            err.IsError = true;
//                            err.Message += String.Format("\t{0} Gantry: {1:f2} [deg], X-Jaw aperture: {2} [cm]。\n", beam.Id, cp.GantryAngle, x_aperture/10);
                            err.Message += String.Format("\t{0} Gantry: {1:f2} [deg], X-Jaw開度: {2} [cm]。\n", beam.Id, cp.GantryAngle, x_aperture/10);
                        }
                        else {; }

                        if (y_aperture < MINIMUM_APERTURE_MM)
                        {
                            err.IsError = true;
//                            err.Message += String.Format("\t{0} Gantry: {1:f2} [deg], Y-Jaw aperture: {2} [cm]。\n", beam.Id, cp.GantryAngle, y_aperture/10);
                            err.Message += String.Format("\t{0} Gantry: {1:f2} [deg], Y-Jaw開度: {2} [cm]。\n", beam.Id, cp.GantryAngle, y_aperture/10);
                        }
                        else {; }

                    }
                }

                if(err.IsError)
                {
//                    err.Message = String.Format("・Jaw aperture is below 3 cm.\n") + err.Message;
                    err.Message = String.Format("・Jawサイズが3cmを下回っています。\n") + err.Message;
                }

                return err;
            }


        }
    }
}
