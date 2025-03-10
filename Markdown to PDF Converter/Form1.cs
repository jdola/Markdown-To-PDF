using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using Markdig;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System.Text;

namespace Markdown_to_PDF_Converter
{
    public partial class Form1 : Form
    {
        private string htmlContent = string.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            try
            {
                string markdownText = rtbMarkdownInput.Text;

                if (string.IsNullOrWhiteSpace(markdownText))
                {
                    MessageBox.Show("Please enter some Markdown text.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Convert Markdown to HTML using Markdig
                var pipeline = new MarkdownPipelineBuilder()
                    .UseAdvancedExtensions()
                    .Build();

                htmlContent = Markdig.Markdown.ToHtml(markdownText, pipeline);

                // Add some basic styling to the HTML
                htmlContent = $@"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 20px; }}
        h1 {{ color: #333366; }}
        h2 {{ color: #333366; }}
        h3 {{ color: #333366; }}
        pre {{ background-color: #f5f5f5; padding: 10px; border-radius: 5px; }}
        code {{ font-family: Consolas, monospace; }}
        blockquote {{ border-left: 4px solid #ccc; padding-left: 15px; color: #666; }}
        table {{ border-collapse: collapse; width: 100%; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; }}
        th {{ background-color: #f2f2f2; }}
    </style>
</head>
<body>
{htmlContent}
</body>
</html>";

                wbPreview.DocumentText = htmlContent;
                tabControl1.SelectedTab = tabPreview;

                statusLabel.Text = "Markdown converted to HTML. Click 'Save as PDF' to export.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error converting Markdown to HTML: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Error occurred during conversion.";
            }
        }

        private void btnSavePdf_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
            {
                MessageBox.Show("Please convert Markdown to HTML first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Save PDF File",
                DefaultExt = "pdf",
                AddExtension = true
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ConvertHtmlToPdf(htmlContent, saveFileDialog.FileName);
                    statusLabel.Text = $"PDF saved successfully to: {saveFileDialog.FileName}";

                    if (MessageBox.Show("PDF saved successfully. Do you want to open it?", "Success",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(saveFileDialog.FileName);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    statusLabel.Text = "Error occurred while saving PDF.";
                }
            }
        }

        private void ConvertHtmlToPdf(string html, string outputPath)
        {
            using (FileStream fs = new FileStream(outputPath, FileMode.Create))
            {
                // Create a PDF document
                Document document = new Document(PageSize.A4, 50, 50, 60, 60);
                PdfWriter writer = PdfWriter.GetInstance(document, fs);
                document.Open();

                // Convert HTML to PDF
                using (StringReader sr = new StringReader(html))
                {
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, sr);
                }

                document.Close();
                writer.Close();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            rtbMarkdownInput.Clear();
            wbPreview.DocumentText = "";
            htmlContent = string.Empty;
            statusLabel.Text = "Ready";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            rtbMarkdownInput.Text = "# Markdown to PDF Converter\n\nThis is a sample Markdown document to demonstrate the conversion functionality.\n\n## Features\n\n- Convert Markdown to HTML\n- Preview HTML rendering\n- Export to PDF\n\n## How to use\n\n1. Type or paste your Markdown text in the input area\n2. Click 'Convert to HTML' to preview\n3. Click 'Save as PDF' to export\n\n## Code Example\n\n```csharp\npublic class Example\n{\n    public static void Main()\n    {\n        Console.WriteLine(\"Hello, World!\");\n    }\n}\n```\n\n## Table Example\n\n| Name | Description |\n|------|-------------|\n| Markdown | Lightweight markup language |\n| PDF | Portable Document Format |\n\n> This is a blockquote example.";

            statusLabel.Text = "Ready";
        }

        private void LoadMarkdownFile(string filePath)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                rtbMarkdownInput.Text = content;
                statusLabel.Text = $"Loaded: {Path.GetFileName(filePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Markdown files (*.md;*.markdown)|*.md;*.markdown|Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Open Markdown File"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                LoadMarkdownFile(openFileDialog.FileName);
            }
        }
    }
}
