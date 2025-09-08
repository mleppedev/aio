# Python + .NET Integrations

## Contexto y Propósito

### ¿Qué es?
La integración de **Python y .NET** permite aprovechar lo mejor de ambos ecosistemas: la flexibilidad y rapidez de scripting de Python con la robustez de .NET en aplicaciones empresariales. Puede lograrse mediante interoperabilidad directa (IronPython, Python.NET), servicios intermedios (REST/gRPC) o uso de Python como motor de procesamiento dentro de pipelines de datos.

### ¿Por qué?
Porque muchos proyectos requieren procesamiento ligero o automatizaciones que son más rápidas de construir en Python. Al mismo tiempo, el núcleo de la aplicación puede seguir en .NET para asegurar escalabilidad y mantenibilidad. El cargo menciona explícitamente **Python 3.7+**, lo que refleja su uso como complemento en integraciones.

### ¿Para qué?
- **Automatizar ETL (Extract-Transform-Load)** para limpiar y preparar datos antes de ingresarlos a SQL Server o Cosmos DB.  
- **Exponer scripts Python** como servicios consumidos desde .NET via REST o gRPC (Llamada a Procedimiento Remoto).  
- **Usar librerías científicas** (NumPy, Pandas) para análisis que luego se integran con aplicaciones .NET.  
- **Orquestar pipelines** en Azure Functions o Azure Data Factory donde Python y .NET coexisten.  

### Valor agregado desde la experiencia
- Implementar **scripts en Python para validación de datos** antes de cargarlos en SQL Server redujo errores de integridad.  
- Integrar **Pandas para limpieza de datos** en pipelines que luego se exponían a APIs .NET.  
- Usar **Azure Functions en Python** permitió procesar archivos en Blob Storage y enviar resultados a aplicaciones .NET.  
- Diseñar **servicios híbridos .NET + Python** habilitó cálculos avanzados (machine learning) reutilizados en APIs REST corporativas.  

# Python + .NET Integrations

## 1. Conceptos Fundamentales

### Interoperabilidad Python/.NET
| Tecnología | Descripción | Casos de Uso |
|------------|-------------|--------------|
| **Python.NET** | Biblioteca que permite ejecutar código Python desde .NET | Scripts de ML, procesamiento de datos |
| **IronPython** | Implementación de Python en .NET Framework | Scripting integrado en aplicaciones .NET |
| **Process Execution** | Ejecutar scripts Python como procesos externos | ETL (Extracción, Transformación, Carga), automatización, validación de datos |
| **REST APIs** | Comunicación via HTTP entre servicios Python y .NET | Microservicios, arquitecturas híbridas |

## 2. Python.NET - Configuración y Uso

### Instalación y Setup
```csharp
// NuGet: Python.Runtime
using Python.Runtime;

public class PythonIntegration
{
    static PythonIntegration()
    {
        // Configurar path de Python
        Runtime.PythonDLL = @"C:\Python39\python39.dll";
        PythonEngine.Initialize();
    }
    
    public static void Cleanup()
    {
        PythonEngine.Shutdown();
    }
}
```

### Ejecutar Scripts Python
```csharp
public class PythonScriptRunner
{
    public dynamic ExecutePythonScript(string script, Dictionary<string, object> variables = null)
    {
        using (Py.GIL())
        {
            using (PyScope scope = Py.CreateScope())
            {
                // Pasar variables a Python
                if (variables != null)
                {
                    foreach (var kvp in variables)
                    {
                        scope.Set(kvp.Key, kvp.Value.ToPython());
                    }
                }
                
                // Ejecutar script
                scope.Exec(script);
                
                // Obtener resultado
                return scope.Get("result")?.As<dynamic>();
            }
        }
    }
}
```

## 3. Procesamiento de Datos con Python

### ETL Pipeline Integration
```csharp
public class DataProcessingPipeline
{
    private readonly PythonScriptRunner _pythonRunner;
    
    public async Task<ProcessedData> ProcessDataAsync(RawData rawData)
    {
        var pythonScript = @"
import pandas as pd
import numpy as np
from sklearn.preprocessing import StandardScaler

# Convertir datos a DataFrame
df = pd.DataFrame(data)

# Limpieza de datos
df = df.dropna()
df = df[df['value'] > 0]

# Transformaciones
scaler = StandardScaler()
df['normalized_value'] = scaler.fit_transform(df[['value']])

# Agregaciones
result = {
    'count': len(df),
    'mean': df['value'].mean(),
    'std': df['value'].std(),
    'processed_data': df.to_dict('records')
}
";
        
        var variables = new Dictionary<string, object>
        {
            ["data"] = rawData.Records.Select(r => new { 
                id = r.Id, 
                value = r.Value, 
                timestamp = r.Timestamp 
            }).ToArray()
        };
        
        var result = _pythonRunner.ExecutePythonScript(pythonScript, variables);
        
        return new ProcessedData
        {
            Count = result.count,
            Mean = result.mean,
            StandardDeviation = result.std,
            ProcessedRecords = JsonSerializer.Deserialize<List<ProcessedRecord>>(
                JsonSerializer.Serialize(result.processed_data))
        };
    }
}
```

### Machine Learning Integration
```csharp
public class MLModelService
{
    public async Task<PredictionResult> PredictAsync(ModelInput input)
    {
        var pythonScript = @"
import joblib
import numpy as np
from sklearn.ensemble import RandomForestClassifier

# Cargar modelo pre-entrenado
model = joblib.load('model.pkl')

# Preparar features
features = np.array(input_features).reshape(1, -1)

# Realizar predicción
prediction = model.predict(features)[0]
probability = model.predict_proba(features)[0].max()

result = {
    'prediction': int(prediction),
    'confidence': float(probability),
    'feature_importance': model.feature_importances_.tolist()
}
";
        
        var variables = new Dictionary<string, object>
        {
            ["input_features"] = new[] { 
                input.Feature1, input.Feature2, input.Feature3 
            }
        };
        
        var result = _pythonRunner.ExecutePythonScript(pythonScript, variables);
        
        return new PredictionResult
        {
            Prediction = result.prediction,
            Confidence = result.confidence,
            FeatureImportance = result.feature_importance
        };
    }
}
```

## 4. Process Execution Approach

### Python Script Execution
```csharp
public class PythonProcessRunner
{
    public async Task<ProcessResult> RunScriptAsync(string scriptPath, 
        string arguments = "", 
        CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"{scriptPath} {arguments}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        
        using var process = new Process { StartInfo = startInfo };
        
        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();
        
        process.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null) outputBuilder.AppendLine(e.Data);
        };
        
        process.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null) errorBuilder.AppendLine(e.Data);
        };
        
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        
        await process.WaitForExitAsync(cancellationToken);
        
        return new ProcessResult
        {
            ExitCode = process.ExitCode,
            Output = outputBuilder.ToString(),
            Error = errorBuilder.ToString(),
            Success = process.ExitCode == 0
        };
    }
}
```

### Data Validation Pipeline
```csharp
public class DataValidationService
{
    private readonly PythonProcessRunner _processRunner;
    
    public async Task<ValidationResult> ValidateDataAsync(string dataFilePath)
    {
        // Script Python para validación
        var validationScript = @"
import sys
import pandas as pd
import json

def validate_data(file_path):
    try:
        df = pd.read_csv(file_path)
        
        validation_results = {
            'total_rows': len(df),
            'null_counts': df.isnull().sum().to_dict(),
            'duplicate_rows': df.duplicated().sum(),
            'data_types': df.dtypes.astype(str).to_dict(),
            'is_valid': True,
            'errors': []
        }
        
        # Validaciones específicas
        if df.empty:
            validation_results['is_valid'] = False
            validation_results['errors'].append('Dataset está vacío')
        
        if df.isnull().sum().sum() > len(df) * 0.5:
            validation_results['is_valid'] = False
            validation_results['errors'].append('Demasiados valores nulos')
        
        return validation_results
        
    except Exception as e:
        return {
            'is_valid': False,
            'errors': [str(e)]
        }

if __name__ == '__main__':
    result = validate_data(sys.argv[1])
    print(json.dumps(result))
";
        
        // Escribir script temporal
        var scriptPath = Path.GetTempFileName().Replace(".tmp", ".py");
        await File.WriteAllTextAsync(scriptPath, validationScript);
        
        try
        {
            var result = await _processRunner.RunScriptAsync(scriptPath, dataFilePath);
            
            if (result.Success)
            {
                return JsonSerializer.Deserialize<ValidationResult>(result.Output);
            }
            
            return new ValidationResult 
            { 
                IsValid = false, 
                Errors = new[] { result.Error } 
            };
        }
        finally
        {
            File.Delete(scriptPath);
        }
    }
}
```

## 5. REST API Integration

### Python Service Client
```csharp
public class PythonServiceClient
{
    private readonly HttpClient _httpClient;
    
    public PythonServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<AnalysisResult> AnalyzeDataAsync(DataPayload payload)
    {
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("/analyze", content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AnalysisResult>(responseJson);
    }
}

// Registro en DI
services.AddHttpClient<PythonServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:8000");
    client.Timeout = TimeSpan.FromMinutes(5);
});
```

### Python FastAPI Service (Ejemplo)
```python
# requirements.txt: fastapi uvicorn pandas numpy scikit-learn

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
import pandas as pd
import numpy as np
from typing import List

app = FastAPI()

class DataRecord(BaseModel):
    id: int
    value: float
    category: str

class DataPayload(BaseModel):
    records: List[DataRecord]

class AnalysisResult(BaseModel):
    summary: dict
    insights: List[str]
    recommendations: List[str]

@app.post("/analyze", response_model=AnalysisResult)
async def analyze_data(payload: DataPayload):
    try:
        # Convertir a DataFrame
        df = pd.DataFrame([record.dict() for record in payload.records])
        
        # Análisis
        summary = {
            "total_records": len(df),
            "mean_value": df['value'].mean(),
            "std_value": df['value'].std(),
            "categories": df['category'].unique().tolist()
        }
        
        insights = [
            f"Dataset contiene {len(df)} registros",
            f"Valor promedio: {summary['mean_value']:.2f}",
            f"Categorías encontradas: {len(summary['categories'])}"
        ]
        
        recommendations = [
            "Considerar normalización de datos" if summary['std_value'] > 100 else "Datos bien distribuidos",
            "Revisar outliers" if df['value'].quantile(0.95) > df['value'].mean() * 3 else "Sin outliers significativos"
        ]
        
        return AnalysisResult(
            summary=summary,
            insights=insights,
            recommendations=recommendations
        )
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
```

## 6. Azure Functions Integration

### Python Function triggered from .NET
```csharp
public class AzureFunctionService
{
    private readonly HttpClient _httpClient;
    
    public async Task<ProcessingResult> TriggerPythonFunctionAsync(ProcessingRequest request)
    {
        var functionUrl = "https://my-python-functions.azurewebsites.net/api/process-data";
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(functionUrl, content);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ProcessingResult>(result);
    }
}
```

## 7. Best Practices y Patrones

### Error Handling
```csharp
public class RobustPythonIntegration
{
    public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
    {
        var delay = TimeSpan.FromSeconds(1);
        
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); // Exponential backoff
            }
        }
        
        return await operation(); // Last attempt without catching
    }
}
```

### Configuration Management
```csharp
public class PythonIntegrationOptions
{
    public string PythonExecutablePath { get; set; } = "python";
    public string PythonDllPath { get; set; }
    public TimeSpan ScriptTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public string TempScriptDirectory { get; set; } = Path.GetTempPath();
    public bool EnablePythonNET { get; set; } = true;
}

// Startup.cs
services.Configure<PythonIntegrationOptions>(Configuration.GetSection("PythonIntegration"));
services.AddSingleton<PythonScriptRunner>();
services.AddScoped<DataProcessingPipeline>();
```

## 8. Testing Strategies

### Unit Testing Python Integration
```csharp
[TestClass]
public class PythonIntegrationTests
{
    [TestMethod]
    public void ExecutePythonScript_SimpleCalculation_ReturnsExpectedResult()
    {
        // Arrange
        var runner = new PythonScriptRunner();
        var script = "result = 2 + 3";
        
        // Act
        var result = runner.ExecutePythonScript(script);
        
        // Assert
        Assert.AreEqual(5, result);
    }
    
    [TestMethod]
    public async Task ProcessDataAsync_ValidData_ReturnsProcessedResult()
    {
        // Arrange
        var pipeline = new DataProcessingPipeline();
        var rawData = new RawData
        {
            Records = new[]
            {
                new RawRecord { Id = 1, Value = 10.5, Timestamp = DateTime.Now },
                new RawRecord { Id = 2, Value = 15.2, Timestamp = DateTime.Now }
            }
        };
        
        // Act
        var result = await pipeline.ProcessDataAsync(rawData);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Mean > 0);
    }
}
```

Este documento proporciona una guía completa para la integración entre Python y .NET, cubriendo desde configuración básica hasta patrones avanzados de procesamiento de datos y machine learning.
