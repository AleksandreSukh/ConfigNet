using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConfigNet;
using CSharpFunctionalExtensions;


namespace Sample_MissingValueFiller
{
    public class Program
    {
        static void Main(string[] args)
        {
            var externalConfigPath = @"ConfigTest\Some.External.config";
            var config = ConfigExtensions.ParseConfig<SomeModuleConfig>(externalConfigPath);
            if (config.IsFailure)
                config = ConfigExtensions.GetMissingValues<SomeModuleConfig>(externalConfigPath)
                    .OnSuccess(missingValues => ConfigExtensions.AddMissingValues(externalConfigPath, missingValues, (name, type) => RerieveMissingValue(new KeyValuePair<string, Type>(name, type)).Value)
                        .OnSuccess(() => ConfigExtensions.ParseConfig<SomeModuleConfig>(externalConfigPath))
                    );
            if (config.IsFailure)
                throw new Exception(config.Error);


            //var des = nameValueCollectionNode.DeserializeAsXml<List<add>>(Encoding.UTF8);

        }
        public static Result<string> RerieveMissingValue(KeyValuePair<string, Type> mv)
        {
            bool retry = true;
            string value = null;
            while (retry)
            {
                value = Prompt.ShowDialog(
                    $"Before you continue it's necessary to fill in configuration value of:{mv.Key}",
                    "Configuration value missing");
                if (value == string.Empty)
                {
                    retry = MessageBox.Show(
                                $"Empty value is not valid. {mv.Key}",
                                "Invalid value!", MessageBoxButtons.RetryCancel) == DialogResult.Retry;
                    if (!retry)
                    {
                        var missingVal = mv.Key;
                        MessageBox.Show(
                            $"Configuraion update will be interrupted. Please note that you will not be able to use application without filling these configuration values{missingVal}",
                            "Update cancelled!", MessageBoxButtons.OK);
                        {
                            return Result.Fail<string>($"Configuration values missing:{missingVal}");
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            return Result.Ok(value);
        }
    }
    //This class is saeled in order to avoid unexpected modifications. (e. g. make it immutable)
    internal sealed class SomeModuleConfig
    {
        public readonly bool Feature1Enabled;
        public readonly int Feature1Timeout;
        public readonly string Feature2RequiredParameter;

        public SomeModuleConfig(bool feature1Enabled, int feature1Timeout, string feature2RequiredParameter)
        {
            Feature1Enabled = feature1Enabled;
            Feature1Timeout = feature1Timeout;
            Feature2RequiredParameter = feature2RequiredParameter;
        }
    }

    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            int buttonWidth = 50;
            int marginHorizontal = 50;
            int marginVertical = 50;
            int spaceBetween = 10;

            Label textLabel = new Label() { Left = marginHorizontal, Top = marginVertical, Text = text, Height = 100, Width = 400, TextAlign = ContentAlignment.TopCenter };
            TextBox textBox = new TextBox() { Left = marginHorizontal, Top = textLabel.Top + textLabel.Height + spaceBetween, Width = textLabel.Width };
            Button confirmation = new Button() { Text = "Ok", Left = textBox.Left + textBox.Width / 2 - buttonWidth / 2, Width = buttonWidth, Top = textBox.Top + textBox.Height + spaceBetween, DialogResult = DialogResult.OK };



            Form prompt = new Form()
            {
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            prompt.Width = prompt.Controls.OfType<Control>().Select(c => c.Width).Max() + marginHorizontal * 2;
            prompt.Height = prompt.Controls.OfType<Control>().Select(c => c.Top + c.Height).Max() + marginVertical * 2;


            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : string.Empty;
        }
    }

}
