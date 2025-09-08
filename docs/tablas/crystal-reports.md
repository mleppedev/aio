# Crystal Reports & Reporting en .NET

## Contexto y Propósito

### ¿Qué es?
**Crystal Reports** es una herramienta de generación de reportes que se integra con aplicaciones .NET para producir informes dinámicos en formatos como PDF, Excel y Word. Permite conectar múltiples orígenes de datos (SQL Server, Oracle, XML, APIs) y diseñar reportes con parámetros, fórmulas y secciones personalizadas.

### ¿Por qué?
Porque en sistemas empresariales sigue siendo necesario generar reportes operativos, financieros y de auditoría. Crystal Reports ofrece flexibilidad para integrarse directamente en aplicaciones Windows (WinForms, WPF) y distribuir documentos estandarizados a usuarios o sistemas externos.

### ¿Para qué?
- **Diseñar reportes interactivos** con filtros y parámetros.  
- **Exportar resultados** en múltiples formatos (PDF, Excel, Word).  
- **Integrar reportes en aplicaciones desktop** mediante Crystal Reports Viewer.  
- **Automatizar generación masiva** en procesos batch o servicios Windows.  

### Valor agregado desde la experiencia
- Integrar **Crystal Reports Viewer en WinForms** para visualización dinámica de reportes CAD.  
- Optimizar consultas mediante **stored procedures parametrizados** redujo tiempo de generación en reportes bancarios.  
- Exportar reportes **PDF en batch** desde servicios Windows permitió enviar información crítica a clientes automáticamente.  
- Diseñar plantillas de reportes reutilizables estandarizó la salida documental en proyectos municipales y retail.  

# Crystal Reports & Reporting - 📊 Generación de Reportes en .NET

## 📋 Índice de Contenidos

| Sección | Descripción |
|---------|-------------|
| [💎 Crystal Reports](#crystal) | Crystal Reports en .NET |
| [📋 SSRS](#ssrs) | SQL Server Reporting Services |
| [📄 Report Viewer](#reportviewer) | ReportViewer Control |
| [🎨 Custom Reports](#custom) | Reportes personalizados |
| [📊 Charts & Graphs](#charts) | Gráficos y visualizaciones |
| [📤 Export Formats](#export) | PDF, Excel, Word export |
| [🔧 Parameters](#parameters) | Parámetros dinámicos |
| [⚡ Performance](#performance) | Optimización de reportes |

---

## 💎 Crystal Reports {#crystal}

### Configuración Inicial

```csharp
// Instalación de Crystal Reports
// Install-Package CrystalReports.Engine
// Install-Package CrystalReports.Shared

public class CrystalReportManager
{
    private ReportDocument _reportDocument;
    
    public void LoadReport(string reportPath)
    {
        _reportDocument = new ReportDocument();
        _reportDocument.Load(reportPath);
    }
    
    public void SetDataSource(DataTable dataTable)
    {
        _reportDocument.SetDataSource(dataTable);
    }
    
    public void SetDatabaseLogon(string server, string database, string userId, string password)
    {
        foreach (CrystalDecisions.CrystalReports.Engine.Table table in _reportDocument.Database.Tables)
        {
            TableLogOnInfo logOnInfo = table.LogOnInfo;
            logOnInfo.ConnectionInfo.ServerName = server;
            logOnInfo.ConnectionInfo.DatabaseName = database;
            logOnInfo.ConnectionInfo.UserID = userId;
            logOnInfo.ConnectionInfo.Password = password;
            table.ApplyLogOnInfo(logOnInfo);
        }
    }
    
    public void ExportToPDF(string filePath)
    {
        _reportDocument.ExportToDisk(ExportFormatType.PortableDocFormat, filePath);
    }
    
    public void Dispose()
    {
        _reportDocument?.Close();
        _reportDocument?.Dispose();
    }
}
```

### Crystal Reports en WinForms

```csharp
public partial class ReportForm : Form
{
    private CrystalReportViewer crystalReportViewer;
    private ReportDocument reportDocument;
    
    public ReportForm()
    {
        InitializeComponent();
        InitializeCrystalReportViewer();
    }
    
    private void InitializeCrystalReportViewer()
    {
        crystalReportViewer = new CrystalReportViewer();
        crystalReportViewer.Dock = DockStyle.Fill;
        crystalReportViewer.ShowCloseButton = false;
        crystalReportViewer.ShowGroupTreeButton = true;
        crystalReportViewer.ShowParameterPanelButton = true;
        crystalReportViewer.ShowRefreshButton = true;
        
        this.Controls.Add(crystalReportViewer);
    }
    
    public void LoadReport(string reportPath, DataSet dataSet)
    {
        try
        {
            reportDocument = new ReportDocument();
            reportDocument.Load(reportPath);
            
            // Configurar fuente de datos
            reportDocument.SetDataSource(dataSet);
            
            // Configurar parámetros si los hay
            SetReportParameters();
            
            // Asignar al viewer
            crystalReportViewer.ReportSource = reportDocument;
            crystalReportViewer.Refresh();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error cargando reporte: {ex.Message}", "Error", 
                           MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void SetReportParameters()
    {
        if (reportDocument.ParameterFields.Count > 0)
        {
            var parameterValues = new ParameterValues();
            var parameterDiscreteValue = new ParameterDiscreteValue();
            
            // Ejemplo: parámetro de fecha
            parameterDiscreteValue.Value = DateTime.Now;
            parameterValues.Add(parameterDiscreteValue);
            
            reportDocument.ParameterFields["FechaReporte"].ApplyCurrentValues(parameterValues);
        }
    }
    
    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        reportDocument?.Close();
        reportDocument?.Dispose();
        base.OnFormClosed(e);
    }
}
```

---

## 📋 SSRS (SQL Server Reporting Services) {#ssrs}

### Configuración SSRS

```csharp
public class SSRSManager
{
    private readonly string _reportServerUrl;
    private readonly string _reportPath;
    
    public SSRSManager(string reportServerUrl, string reportPath)
    {
        _reportServerUrl = reportServerUrl;
        _reportPath = reportPath;
    }
    
    public byte[] RenderReport(string format, Dictionary<string, object> parameters = null)
    {
        using var reportingService = new ReportExecutionServiceSoapClient();
        
        // Configurar credenciales
        reportingService.ClientCredentials.Windows.AllowedImpersonationLevel = 
            System.Security.Principal.TokenImpersonationLevel.Impersonation;
        
        // Cargar reporte
        var executionInfo = reportingService.LoadReport(_reportPath, null);
        
        // Configurar parámetros
        if (parameters != null && parameters.Any())
        {
            var reportParameters = parameters.Select(p => new ParameterValue
            {
                Name = p.Key,
                Value = p.Value?.ToString()
            }).ToArray();
            
            reportingService.SetExecutionParameters(reportParameters, "en-US");
        }
        
        // Renderizar reporte
        var result = reportingService.Render(format, null, out var extension, 
                                           out var mimeType, out var encoding, 
                                           out var warnings, out var streamIds);
        
        return result;
    }
    
    public void ExportReportToPDF(string filePath, Dictionary<string, object> parameters = null)
    {
        var pdfBytes = RenderReport("PDF", parameters);
        File.WriteAllBytes(filePath, pdfBytes);
    }
    
    public void ExportReportToExcel(string filePath, Dictionary<string, object> parameters = null)
    {
        var excelBytes = RenderReport("EXCELOPENXML", parameters);
        File.WriteAllBytes(filePath, excelBytes);
    }
}
```

---

## 📄 Report Viewer Control {#reportviewer}

### Microsoft ReportViewer

```csharp
public partial class ReportViewerForm : Form
{
    private Microsoft.Reporting.WinForms.ReportViewer reportViewer;
    
    public ReportViewerForm()
    {
        InitializeComponent();
        InitializeReportViewer();
    }
    
    private void InitializeReportViewer()
    {
        reportViewer = new Microsoft.Reporting.WinForms.ReportViewer();
        reportViewer.Dock = DockStyle.Fill;
        reportViewer.ProcessingMode = ProcessingMode.Local;
        
        this.Controls.Add(reportViewer);
    }
    
    public void LoadLocalReport(string reportPath, DataTable dataTable, string dataSetName)
    {
        try
        {
            reportViewer.LocalReport.ReportPath = reportPath;
            
            // Limpiar fuentes de datos existentes
            reportViewer.LocalReport.DataSources.Clear();
            
            // Agregar nueva fuente de datos
            var dataSource = new ReportDataSource(dataSetName, dataTable);
            reportViewer.LocalReport.DataSources.Add(dataSource);
            
            // Configurar parámetros
            SetLocalReportParameters();
            
            // Refrescar reporte
            reportViewer.RefreshReport();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error cargando reporte: {ex.Message}", "Error",
                           MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
    
    private void SetLocalReportParameters()
    {
        var parameters = new List<ReportParameter>
        {
            new ReportParameter("FechaGeneracion", DateTime.Now.ToString("dd/MM/yyyy")),
            new ReportParameter("Usuario", Environment.UserName)
        };
        
        reportViewer.LocalReport.SetParameters(parameters);
    }
    
    public void ExportReport(string format, string filePath)
    {
        try
        {
            byte[] bytes = reportViewer.LocalReport.Render(format);
            File.WriteAllBytes(filePath, bytes);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error exportando reporte: {ex.Message}", "Error",
                           MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
```

---

## 🎨 Custom Reports {#custom}

### Generador de Reportes Personalizado

```csharp
public class CustomReportGenerator
{
    public void GenerateCustomReport(List<Customer> customers, string filePath)
    {
        using var document = new iTextSharp.text.Document();
        using var writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
        
        document.Open();
        
        // Título
        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        var title = new Paragraph("Reporte de Clientes", titleFont);
        title.Alignment = Element.ALIGN_CENTER;
        document.Add(title);
        
        // Fecha
        var dateFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
        var date = new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}", dateFont);
        date.Alignment = Element.ALIGN_RIGHT;
        document.Add(date);
        
        document.Add(new Paragraph(" ")); // Espacio
        
        // Tabla
        var table = new PdfPTable(4);
        table.WidthPercentage = 100;
        table.SetWidths(new float[] { 1, 3, 3, 2 });
        
        // Headers
        var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
        table.AddCell(new PdfPCell(new Phrase("ID", headerFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase("Nombre", headerFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase("Email", headerFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
        table.AddCell(new PdfPCell(new Phrase("Teléfono", headerFont)) { BackgroundColor = BaseColor.LIGHT_GRAY });
        
        // Datos
        var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
        foreach (var customer in customers)
        {
            table.AddCell(new PdfPCell(new Phrase(customer.Id.ToString(), cellFont)));
            table.AddCell(new PdfPCell(new Phrase(customer.Name, cellFont)));
            table.AddCell(new PdfPCell(new Phrase(customer.Email, cellFont)));
            table.AddCell(new PdfPCell(new Phrase(customer.Phone ?? "", cellFont)));
        }
        
        document.Add(table);
        
        // Pie de página con estadísticas
        document.Add(new Paragraph(" "));
        var stats = new Paragraph($"Total de clientes: {customers.Count}", dateFont);
        document.Add(stats);
        
        document.Close();
    }
}
```

---

## 📊 Charts & Graphs {#charts}

### Generación de Gráficos

```csharp
public class ChartReportGenerator
{
    public void GenerateChartReport(Dictionary<string, int> data, string filePath)
    {
        using var chart = new Chart();
        chart.Width = 800;
        chart.Height = 600;
        
        // Área del gráfico
        var chartArea = new ChartArea("MainArea");
        chartArea.AxisX.Title = "Categorías";
        chartArea.AxisY.Title = "Valores";
        chart.ChartAreas.Add(chartArea);
        
        // Serie de datos
        var series = new Series("Datos");
        series.ChartType = SeriesChartType.Column;
        series.Color = Color.Blue;
        
        foreach (var item in data)
        {
            series.Points.AddXY(item.Key, item.Value);
        }
        
        chart.Series.Add(series);
        
        // Título
        var title = new Title("Reporte de Ventas por Categoría");
        title.Font = new Font("Arial", 16, FontStyle.Bold);
        chart.Titles.Add(title);
        
        // Leyenda
        var legend = new Legend("MainLegend");
        legend.Docking = Docking.Bottom;
        chart.Legends.Add(legend);
        
        // Guardar como imagen
        chart.SaveImage(filePath, ChartImageFormat.Png);
    }
    
    public void GeneratePieChart(Dictionary<string, double> data, string filePath)
    {
        using var chart = new Chart();
        chart.Width = 600;
        chart.Height = 600;
        
        var chartArea = new ChartArea("PieArea");
        chart.ChartAreas.Add(chartArea);
        
        var series = new Series("PieData");
        series.ChartType = SeriesChartType.Pie;
        series.IsValueShownAsLabel = true;
        series.LabelFormat = "#PERCENT{P1}";
        
        foreach (var item in data)
        {
            var point = series.Points.Add(item.Value);
            point.LegendText = item.Key;
            point.Label = "#PERCENT{P1}";
        }
        
        chart.Series.Add(series);
        
        var legend = new Legend();
        legend.Docking = Docking.Right;
        chart.Legends.Add(legend);
        
        chart.SaveImage(filePath, ChartImageFormat.Png);
    }
}
```

[Continúa con más secciones...]
