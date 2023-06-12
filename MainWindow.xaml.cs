using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using Newtonsoft.Json;


namespace EnumToJSObject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private static EnumDeclarationSyntax GetEnumFromFile(string filePath)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath));
            CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();

            EnumDeclarationSyntax enumDeclaration = root.DescendantNodes().OfType<EnumDeclarationSyntax>().FirstOrDefault();
            return enumDeclaration;
        }

        private void importFileBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new();
                openFileDialog.Filter = "C# Files (*.cs)|*.cs";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                bool? result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    EnumDeclarationSyntax enumDeclaration = GetEnumFromFile(selectedFilePath);
                    if (enumDeclaration != null)
                    {
                        string enumName = enumDeclaration.Identifier.ValueText;

                        Dictionary<string, int> enumValues = new Dictionary<string, int>();
                        foreach (EnumMemberDeclarationSyntax enumMember in enumDeclaration.Members)
                        {
                            string memberName = enumMember.Identifier.ValueText;
                            string memberValue = enumMember.EqualsValue?.Value.ToString() ?? enumMember.EqualsValue?.ChildNodes().FirstOrDefault()?.ToString();

                            int parsedValue;
                            if (int.TryParse(memberValue, out parsedValue))
                            {
                                enumValues.Add(memberName, parsedValue);
                            }
                        }

                        string jsonString = JsonConvert.SerializeObject(enumValues, Formatting.Indented);

                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.Filter = "JavaScript Files (*.js)|*.js";
                        saveFileDialog.FileName = $"{enumName}.js";

                        bool? saveResult = saveFileDialog.ShowDialog();

                        if (saveResult == true)
                        {
                            string saveFilePath = saveFileDialog.FileName;
                            File.WriteAllText(saveFilePath, $"const {enumName} = {jsonString};");
                            resultText.Text = "The enum was successfully converted and written to file. ✅";
                            resultText.Foreground = new SolidColorBrush(SystemColors.HighlightColor);
                            resultText.Visibility = Visibility.Visible;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                resultText.Text = "An error occured during the converting process. ❌";
                resultText.Foreground = new SolidColorBrush(Colors.Red);
                resultText.Visibility = Visibility.Visible;
            }

        }
    }
}
