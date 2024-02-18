using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

using plan_checker_gui_esapi_v15_5;

namespace VMS.TPS
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Script : UserControl
    {
        public Script()
        {
            InitializeComponent();
        }

        public void Execute(ScriptContext context, System.Windows.Window window)
        {
            window.Content = this;
            string setting_file_path = "$YOUR_SETTING_FILE_PATH\\plan_check_parameters.csv";

            PlanCheck.CheckedPlan plancheck = new(context, setting_file_path);
            //            plancheck.show_plan_info();

            /* Load check settings */
            var settings = plancheck.Settings;
            string settings_str =
                String.Format("Settings for plan check from '{0}'\n", setting_file_path)
                + String.Format("\tMlcJawToleranceDistance: {0}\n", settings.mlc_jaw_dist_tole)
                + String.Format("\tPhotonCalcAlgorithmConv: {0}\n", settings.photon_calc_algo_conv)
                + String.Format("\tPhotonCalcAlgorithmImrtSrt: {0}\n", settings.photon_calc_algo_imrt)
                + String.Format("\tElectronCalcAlgorithm: {0}\n", settings.electron_calc_algo)
                + String.Format("\tPhotonCalcGridConv: {0}\n", settings.photon_calc_grid_conv);

            foreach (var bar in settings.photon_calc_grid_imrt)
            {
                settings_str +=
                     String.Format("\tPhotonCalcGridImrtSrt: {0}\n", bar);
            }
            settings_str +=
                String.Format("\tElectronCalcGrid: {0}\n", settings.electron_calc_grid);
            foreach (var bar in settings.dose_rate_dict)
            {
                settings_str +=
                     String.Format("\tDoseRate: Machine = {0}, Energy = {1}, DoseRate = {2}\n", bar.Key.Item1, bar.Key.Item2, bar.Value);
            }
            foreach (var foo in settings.linac_couch_dict)
            {
                foreach(var bar in foo.Value) 
                {
                    settings_str +=
                         String.Format("\tLinacCouchPair: Machine = {0}, Couch = {1}\n", foo.Key, bar);
                }
            }
            this.ParametersTextBlock.Text = settings_str;

            /* Patient and Plan Info. */
            var energies_str = "";
            foreach (var (energy, index) in plancheck.Energies.Select((energy, index) => (energy, index)))
            {
                if (index == 0)
                {
                    energies_str = energy;
                }
                else
                {
                    energies_str += ", " + energy;
                }
            }

            this.PatientName.Text = plancheck.PatientName;
            this.PatientId.Text = plancheck.PatientID;
            this.PlanName.Text = plancheck.PlanID;
            this.Energy.Text = energies_str;
            this.MachineId.Text = plancheck.MachineId;
            this.DosePerFraction.Text = plancheck.DosePerFraction.Dose + " [cGy]";
            this.TotalDoseFraction.Text = plancheck.TotalDose.Dose + "[cGy] / " + plancheck.NumOfFraction + "[fr]";
//            this.TotalDose.Text = plancheck.TotalDose.Dose / 100 + " [Gy]";
//            this.DoseFraction.Text = plancheck.DosePerFraction.Dose / 100 + "[Gy] * " + plancheck.NumOfFraction + "[fr]";

            /* Plan check */
//            string ok_color = "#339900";
//            string warning_color = "#ffcc00";
//            string warning_color = "#ffbb00";
//            string error_color = "#cc3300";
            string message_color = "#101010";
            PlanCheck.Error res;


            // Virtual couch
//                AppendText(this.OkRichTextBox, "Virtual couch for VMAT or SRT...", message_color);
            AppendText(this.OkRichTextBox, "・ Virtual couch があるか (VMAT, SRT 用) ... ", message_color);
            res = plancheck.IsCouchOk;
            HandleCheckResult(res);

            // Jaw and MLC distance
//                AppendText(this.OkRichTextBox, "Jaw and MLC distance...", message_color);
            AppendText(this.OkRichTextBox, "・ Jaw、MLC の開度 ... ", message_color);
            res = plancheck.IsJawMlcOk;
            HandleCheckResult(res);

            // Calculation algorithm
//                AppendText(this.OkRichTextBox, "Calculation algorithm...", message_color);
            AppendText(this.OkRichTextBox, "・ 計算アルゴリズム ... ", message_color);
            res = plancheck.IsCalcAlgorithmOk;
            HandleCheckResult(res);

            // Dose rate
//                AppendText(this.OkRichTextBox, "Dose rate...", message_color);
            AppendText(this.OkRichTextBox, "・ 線量率が最大か ... ", message_color);
            res = plancheck.IsDoseRateOk;
            HandleCheckResult(res);

            // Normalization method
//                AppendText(this.OkRichTextBox, "Normalization method for Conventional irradiation...", message_color);
            AppendText(this.OkRichTextBox, "・ Normalization method (コンベ用) ... ", message_color);
            res = plancheck.IsNormalizationMethodOk;
            HandleCheckResult(res);

            // Isocenter and reference point match
//                AppendText(this.OkRichTextBox, "Isocenter & Reference point match for Conventional irradiation...", message_color);
            AppendText(this.OkRichTextBox, "・ アイソセンタとリファレンスポイントの一致 (コンベ用) ... ", message_color);
            res = plancheck.IsIsocenterRefpointMatchedOk;
            HandleCheckResult(res);
            
            // Isocenter precision
//                AppendText(this.OkRichTextBox, "Isocenter precision...", message_color);
            AppendText(this.OkRichTextBox, "・ アイソセンタの小数点第二位が 0 か ... ", message_color);
            res = plancheck.IsIsocenterPrecisionOk;
            HandleCheckResult(res);

            // Integer gantry angle 
//                AppendText(this.OkRichTextBox, "Integer Gantry angle...", message_color);
            AppendText(this.OkRichTextBox, "・ ガントリ角度が整数か ... ", message_color);
            res = plancheck.IsGantryAnglePrecisionOk;
            HandleCheckResult(res);

            // MU is more than or equal to 10
//            AppendText(this.OkRichTextBox, "・ No field with MU less than 10... ", message_color);
            AppendText(this.OkRichTextBox, "・ MUが10以下のFieldがないか... ", message_color);
            res = plancheck.IsMuOk;
            HandleCheckResult(res);

            // Jaw aperture is more than or equal to 3 cm
//            AppendText(this.OkRichTextBox, "・ No field with Jaw aperture less than 3 cm... ", message_color);
            AppendText(this.OkRichTextBox, "・ Jaw開度が3cm以下のFieldがないか... ", message_color);
            res = plancheck.IsJawApertureOk;
            HandleCheckResult(res);


        }

        public static void AppendText(RichTextBox box, string text, string color)
        {
            BrushConverter bc = new();
            TextRange tr = new(box.Document.ContentEnd, box.Document.ContentEnd);
            try
            {
                tr.Text = text;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, bc.ConvertFromString(color));
                tr.ApplyPropertyValue(TextElement.FontSizeProperty, 16.0);
            }
            catch (FormatException) { }
        }

//        public void HandleCheckResult(in string res)
        internal void HandleCheckResult(in PlanCheck.Error res)
        {
            string ok_color = "#339900";
//            string warning_color = "#ffcc00";
//            string warning_color = "#ffbb00";
            string error_color = "#cc3300";

            if (!res.IsError)
            {
                string buf = (res.Message == "") ? "" : string.Format("{0}", res.Message);
                AppendText(this.OkRichTextBox, string.Format("OK!\n{0}", buf), ok_color);
            }
            else
            {
                AppendText(this.OkRichTextBox, "Warning.\n", error_color);
                AppendText(this.ErrorRichTextBox, res.Message, error_color);
            }

        }

        private void OkRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
