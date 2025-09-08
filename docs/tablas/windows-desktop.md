# Windows Desktop Development en .NET

## Contexto y Propósito

### ¿Qué es?
El desarrollo de aplicaciones de escritorio en Windows con .NET incluye tecnologías como **WinForms**, **WPF (Windows Presentation Foundation)** y **WinUI 3 (Windows App SDK)**. Estas plataformas permiten crear interfaces gráficas (GUI) con soporte para patrones modernos como MVVM (Model-View-ViewModel), integración con servicios, acceso a bases de datos y despliegue en entornos corporativos.

### ¿Por qué?
Porque en muchos sectores (banca, retail, CAD municipal) todavía se requieren aplicaciones ricas en UI, con fuerte integración local y capacidad de trabajar offline. A diferencia de las apps web, las desktop apps permiten interacción directa con hardware, menor latencia en la UI y despliegue controlado en entornos críticos.

### ¿Para qué?
- **WinForms:** ideal para mantenimiento de aplicaciones legacy o proyectos de rápida construcción.  
- **WPF:** para interfaces modernas con bindings avanzados y personalización de estilos.  
- **WinUI 3:** para proyectos nuevos con soporte de Windows App SDK y mayor vida útil.  
- **MVVM:** separación clara entre UI y lógica de negocio, mejorando testabilidad y mantenibilidad.  

### Valor agregado desde la experiencia
- Migrar aplicaciones legacy de **WinForms .NET Framework** a **WPF .NET 6+** con MVVM redujo deuda técnica y mejoró la UX.  
- Implementar **binding y validación en WPF** redujo errores de UI y duplicación de código.  
- Desarrollar prototipos rápidos en **WinForms** permitió entregar valor en semanas, manteniendo escalabilidad futura con refactor a MVVM.  
- Con **WinUI 3**, implementamos apps CAD con soporte de XAML moderno y comunicación con APIs REST.  

# Windows Desktop Development - 🖥️ WinForms, WPF, WinUI 3 & MVVM

## 📋 Índice de Contenidos

| Sección | Descripción |
|---------|-------------|
| [🖼️ WinForms](#winforms) | Windows Forms - UI tradicional |
| [🎨 WPF](#wpf) | Windows Presentation Foundation |
| [⚡ WinUI 3](#winui3) | Modern Windows UI Framework |
| [🏗️ MVVM](#mvvm) | Model-View-ViewModel Pattern |
| [📡 Data Binding](#databinding) | Enlace de datos y sincronización |
| [🎯 Commands](#commands) | Command Pattern y ICommand |
| [🔄 Dependency Injection](#di) | DI en aplicaciones desktop |
| [🎪 Controles Avanzados](#controles) | DataGrid, TreeView, CustomControls |
| [🔔 Notifications](#notifications) | Toast notifications y system tray |
| [📦 Deployment](#deployment) | Distribución y packaging |

---

## 🖼️ WinForms {#winforms}

### Conceptos Fundamentales

| Concepto | Descripción | Ejemplo de Uso |
|----------|-------------|----------------|
| **Form** | Ventana principal de la aplicación | `public partial class MainForm : Form` |
| **Controls** | Elementos UI (Button, TextBox, etc.) | `Button btnSave = new Button()` |
| **Events** | Manejo de eventos del usuario | `btnSave.Click += BtnSave_Click` |
| **Designer** | Editor visual de formularios | Drag & drop en Visual Studio |

### Arquitectura WinForms

```csharp
// Formulario principal con patrón MVP
public partial class MainForm : Form, IMainView
{
    private readonly IMainPresenter _presenter;
    
    public MainForm(IMainPresenter presenter)
    {
        InitializeComponent();
        _presenter = presenter;
        _presenter.SetView(this);
    }
    
    private void MainForm_Load(object sender, EventArgs e)
    {
        _presenter.LoadData();
    }
    
    public void DisplayData(List<Customer> customers)
    {
        dataGridView1.DataSource = customers;
    }
    
    public void ShowMessage(string message)
    {
        MessageBox.Show(message);
    }
}

// Presenter para separar lógica de negocio
public class MainPresenter : IMainPresenter
{
    private IMainView _view;
    private readonly ICustomerService _customerService;
    
    public MainPresenter(ICustomerService customerService)
    {
        _customerService = customerService;
    }
    
    public void SetView(IMainView view)
    {
        _view = view;
    }
    
    public async void LoadData()
    {
        try
        {
            var customers = await _customerService.GetAllAsync();
            _view.DisplayData(customers);
        }
        catch (Exception ex)
        {
            _view.ShowMessage($"Error: {ex.Message}");
        }
    }
}
```

### Custom Controls en WinForms

```csharp
// Control personalizado con eventos
public partial class CustomTextBox : UserControl
{
    public event EventHandler<string> TextValidated;
    
    public string ValidationPattern { get; set; }
    
    private void textBox1_Validating(object sender, CancelEventArgs e)
    {
        if (!string.IsNullOrEmpty(ValidationPattern))
        {
            if (!Regex.IsMatch(textBox1.Text, ValidationPattern))
            {
                e.Cancel = true;
                errorProvider1.SetError(textBox1, "Formato inválido");
                return;
            }
        }
        
        errorProvider1.SetError(textBox1, "");
        TextValidated?.Invoke(this, textBox1.Text);
    }
    
    // Propiedades personalizadas
    [Category("Custom")]
    [Description("Texto del control")]
    public string CustomText
    {
        get => textBox1.Text;
        set => textBox1.Text = value;
    }
}
```

---

## 🎨 WPF {#wpf}

### Conceptos Fundamentals WPF

| Concepto | Descripción | Características |
|----------|-------------|-----------------|
| **XAML** | Markup language para UI | Declarativo, separación UI/lógica |
| **Dependency Properties** | Sistema de propiedades avanzado | Data binding, styling, animation |
| **Routed Events** | Sistema de eventos WPF | Bubbling, tunneling, direct |
| **Resources** | Reutilización de elementos | Styles, templates, converters |

### Arquitectura MVVM en WPF

```csharp
// ViewModel base con INotifyPropertyChanged
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
            return false;
            
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

// ViewModel principal
public class MainWindowViewModel : ViewModelBase
{
    private string _title = "Mi Aplicación WPF";
    private ObservableCollection<Customer> _customers;
    private Customer _selectedCustomer;
    
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
    
    public ObservableCollection<Customer> Customers
    {
        get => _customers;
        set => SetProperty(ref _customers, value);
    }
    
    public Customer SelectedCustomer
    {
        get => _selectedCustomer;
        set => SetProperty(ref _selectedCustomer, value);
    }
    
    // Commands
    public ICommand LoadDataCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }
    
    public MainWindowViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
        
        LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
        SaveCommand = new RelayCommand(async () => await SaveAsync(), CanSave);
        DeleteCommand = new RelayCommand(async () => await DeleteAsync(), CanDelete);
        
        Customers = new ObservableCollection<Customer>();
    }
    
    private async Task LoadDataAsync()
    {
        var customers = await _customerService.GetAllAsync();
        Customers.Clear();
        foreach (var customer in customers)
        {
            Customers.Add(customer);
        }
    }
    
    private bool CanSave() => SelectedCustomer != null && SelectedCustomer.IsValid;
    private bool CanDelete() => SelectedCustomer != null;
}
```

### XAML Avanzado

```xml
<!-- MainWindow.xaml -->
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:converters="clr-namespace:MyApp.Converters"
        Title="{Binding Title}" Height="600" Width="800">
    
    <Window.Resources>
        <converters:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
        <converters:NullToBoolConverter x:Key="NullToBoolConverter"/>
        
        <!-- Estilos globales -->
        <Style TargetType="Button" x:Key="PrimaryButton">
            <Setter Property="Background" Value="#007ACC"/>
            <Setter Property="Foreground" Value="White"/>
            <Setter Property="Padding" Value="10,5"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="Button">
                        <Border Background="{TemplateBinding Background}"
                                CornerRadius="3">
                            <ContentPresenter HorizontalAlignment="Center"
                                            VerticalAlignment="Center"/>
                        </Border>
                        <ControlTemplate.Triggers>
                            <Trigger Property="IsMouseOver" Value="True">
                                <Setter Property="Background" Value="#005a9e"/>
                            </Trigger>
                        </ControlTemplate.Triggers>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>
    </Window.Resources>
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- Toolbar -->
        <ToolBar Grid.Row="0">
            <Button Content="Cargar" Command="{Binding LoadDataCommand}" 
                    Style="{StaticResource PrimaryButton}"/>
            <Button Content="Guardar" Command="{Binding SaveCommand}"
                    IsEnabled="{Binding SelectedCustomer, Converter={StaticResource NullToBoolConverter}}"/>
            <Button Content="Eliminar" Command="{Binding DeleteCommand}"/>
        </ToolBar>
        
        <!-- Content Area -->
        <Grid Grid.Row="1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="300"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>
            
            <!-- Lista de clientes -->
            <DataGrid Grid.Column="0" 
                      ItemsSource="{Binding Customers}"
                      SelectedItem="{Binding SelectedCustomer}"
                      AutoGenerateColumns="False">
                <DataGrid.Columns>
                    <DataGridTextColumn Header="ID" Binding="{Binding Id}" Width="50"/>
                    <DataGridTextColumn Header="Nombre" Binding="{Binding Name}" Width="*"/>
                    <DataGridTextColumn Header="Email" Binding="{Binding Email}" Width="*"/>
                </DataGrid.Columns>
            </DataGrid>
            
            <!-- Detalle del cliente -->
            <ScrollViewer Grid.Column="1" Margin="10">
                <StackPanel DataContext="{Binding SelectedCustomer}"
                           Visibility="{Binding Converter={StaticResource NullToBoolConverter}, 
                                               ConverterParameter=Collapsed}">
                    <TextBlock Text="Detalles del Cliente" FontSize="18" FontWeight="Bold"/>
                    
                    <Grid Margin="0,10">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="100"/>
                            <ColumnDefinition Width="*"/>
                        </Grid.ColumnDefinitions>
                        <Grid.RowDefinitions>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                            <RowDefinition Height="Auto"/>
                        </Grid.RowDefinitions>
                        
                        <TextBlock Grid.Row="0" Grid.Column="0" Text="Nombre:"/>
                        <TextBox Grid.Row="0" Grid.Column="1" Text="{Binding Name, UpdateSourceTrigger=PropertyChanged}"/>
                        
                        <TextBlock Grid.Row="1" Grid.Column="0" Text="Email:"/>
                        <TextBox Grid.Row="1" Grid.Column="1" Text="{Binding Email, UpdateSourceTrigger=PropertyChanged}"/>
                        
                        <TextBlock Grid.Row="2" Grid.Column="0" Text="Teléfono:"/>
                        <TextBox Grid.Row="2" Grid.Column="1" Text="{Binding Phone, UpdateSourceTrigger=PropertyChanged}"/>
                    </Grid>
                </StackPanel>
            </ScrollViewer>
        </Grid>
        
        <!-- Status Bar -->
        <StatusBar Grid.Row="2">
            <StatusBarItem Content="{Binding Customers.Count, StringFormat='Total: {0} clientes'}"/>
        </StatusBar>
    </Grid>
</Window>
```

---

## ⚡ WinUI 3 {#winui3}

### Características WinUI 3

| Característica | Descripción | Ventajas |
|----------------|-------------|----------|
| **Modern XAML** | XAML actualizado y optimizado | Mejor rendimiento, nuevos controles |
| **Win32 Support** | Funciona en aplicaciones Win32 | No requiere UWP |
| **Fluent Design** | Sistema de diseño de Microsoft | UI moderna y consistente |
| **Performance** | Renderizado optimizado | Mejor que WPF en muchos casos |

### Aplicación WinUI 3 con MVVM

```csharp
// App.xaml.cs - Configuración de DI
public partial class App : Application
{
    private ServiceProvider _serviceProvider;
    
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // Configurar servicios
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
        
        // Crear ventana principal
        _window = _serviceProvider.GetRequiredService<MainWindow>();
        _window.Activate();
    }
    
    private void ConfigureServices(ServiceCollection services)
    {
        // Views
        services.AddTransient<MainWindow>();
        
        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        
        // Services
        services.AddSingleton<ICustomerService, CustomerService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<INavigationService, NavigationService>();
    }
}

// MainWindow.xaml.cs
public sealed partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; }
    
    public MainWindow(MainWindowViewModel viewModel)
    {
        this.InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
    }
}

// ViewModel específico para WinUI 3
public class MainWindowViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private readonly IDialogService _dialogService;
    
    public ObservableCollection<Customer> Customers { get; } = new();
    
    [ObservableProperty]
    private Customer selectedCustomer;
    
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            var customers = await _customerService.GetAllAsync();
            Customers.Clear();
            foreach (var customer in customers)
            {
                Customers.Add(customer);
            }
        }
        catch (Exception ex)
        {
            await _dialogService.ShowErrorAsync("Error", ex.Message);
        }
    }
    
    [RelayCommand(CanExecute = nameof(CanSaveCustomer))]
    private async Task SaveCustomerAsync()
    {
        await _customerService.SaveAsync(SelectedCustomer);
        await _dialogService.ShowInfoAsync("Éxito", "Cliente guardado correctamente");
    }
    
    private bool CanSaveCustomer() => SelectedCustomer?.IsValid == true;
}
```

### WinUI 3 XAML Moderno

```xml
<!-- MainWindow.xaml -->
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        
        <!-- Navigation View -->
        <NavigationView Grid.Row="0" Grid.RowSpan="2"
                       IsBackButtonVisible="Collapsed"
                       PaneDisplayMode="Left">
            <NavigationView.MenuItems>
                <NavigationViewItem Content="Clientes" Icon="People" Tag="customers"/>
                <NavigationViewItem Content="Productos" Icon="Shop" Tag="products"/>
                <NavigationViewItem Content="Reportes" Icon="Document" Tag="reports"/>
            </NavigationView.MenuItems>
            
            <NavigationView.Content>
                <Frame x:Name="ContentFrame"/>
            </NavigationView.Content>
        </NavigationView>
    </Grid>
</Window>

<!-- CustomerPage.xaml -->
<Page x:Class="MyApp.Views.CustomerPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    
    <Grid Padding="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        
        <!-- Comandos -->
        <CommandBar Grid.Row="0">
            <AppBarButton Icon="Add" Label="Nuevo" Command="{Binding AddCommand}"/>
            <AppBarButton Icon="Save" Label="Guardar" Command="{Binding SaveCommand}"/>
            <AppBarButton Icon="Delete" Label="Eliminar" Command="{Binding DeleteCommand}"/>
            <AppBarSeparator/>
            <AppBarButton Icon="Refresh" Label="Actualizar" Command="{Binding LoadDataCommand}"/>
        </CommandBar>
        
        <!-- Contenido principal -->
        <Grid Grid.Row="1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="400"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>
            
            <!-- Lista con búsqueda -->
            <Grid Grid.Column="0">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="*"/>
                </Grid.RowDefinitions>
                
                <AutoSuggestBox Grid.Row="0" 
                               PlaceholderText="Buscar clientes..."
                               Text="{Binding SearchText, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
                               Margin="0,0,0,10"/>
                
                <ListView Grid.Row="1"
                         ItemsSource="{Binding FilteredCustomers}"
                         SelectedItem="{Binding SelectedCustomer, Mode=TwoWay}">
                    <ListView.ItemTemplate>
                        <DataTemplate>
                            <Grid Padding="10">
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="Auto"/>
                                    <ColumnDefinition Width="*"/>
                                </Grid.ColumnDefinitions>
                                
                                <PersonPicture Grid.Column="0" Width="40" Height="40"
                                             ProfilePicture="{Binding Avatar}"/>
                                
                                <StackPanel Grid.Column="1" Margin="10,0,0,0">
                                    <TextBlock Text="{Binding Name}" FontWeight="Bold"/>
                                    <TextBlock Text="{Binding Email}" 
                                             Foreground="{ThemeResource SystemBaseMediumColor}"/>
                                </StackPanel>
                            </Grid>
                        </DataTemplate>
                    </ListView.ItemTemplate>
                </ListView>
            </Grid>
            
            <!-- Detalles -->
            <ScrollViewer Grid.Column="1" Margin="20,0,0,0">
                <StackPanel DataContext="{Binding SelectedCustomer}" Spacing="15">
                    <TextBlock Text="Información del Cliente" 
                             Style="{StaticResource TitleTextBlockStyle}"/>
                    
                    <TextBox Header="Nombre" Text="{Binding Name, Mode=TwoWay}"/>
                    <TextBox Header="Email" Text="{Binding Email, Mode=TwoWay}"/>
                    <TextBox Header="Teléfono" Text="{Binding Phone, Mode=TwoWay}"/>
                    
                    <DatePicker Header="Fecha de Registro" 
                               Date="{Binding RegisterDate, Mode=TwoWay}"/>
                    
                    <ToggleSwitch Header="Cliente Activo" 
                                 IsOn="{Binding IsActive, Mode=TwoWay}"/>
                </StackPanel>
            </ScrollViewer>
        </Grid>
    </Grid>
</Page>
```

---

## 🏗️ MVVM Pattern {#mvvm}

### Componentes MVVM

| Componente | Responsabilidad | Implementación |
|------------|-----------------|----------------|
| **Model** | Datos y lógica de negocio | Entities, Services, DTOs |
| **View** | Interfaz de usuario | XAML, Code-behind mínimo |
| **ViewModel** | Lógica de presentación | Commands, Properties, Validation |

### RelayCommand Implementación

```csharp
// Command base reutilizable
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;
    
    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
    
    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }
    
    public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
    
    public void Execute(object parameter) => _execute(parameter);
    
    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}

// Async Command
public class AsyncRelayCommand : ICommand
{
    private readonly Func<object, Task> _execute;
    private readonly Func<object, bool> _canExecute;
    private bool _isExecuting;
    
    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
    
    public AsyncRelayCommand(Func<object, Task> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }
    
    public bool CanExecute(object parameter)
    {
        return !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);
    }
    
    public async void Execute(object parameter)
    {
        if (!CanExecute(parameter)) return;
        
        try
        {
            _isExecuting = true;
            RaiseCanExecuteChanged();
            await _execute(parameter);
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }
    
    public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}
```

### Validation en MVVM

```csharp
// ViewModel con validación
public class CustomerViewModel : ViewModelBase, IDataErrorInfo
{
    private string _name;
    private string _email;
    private readonly Dictionary<string, string> _errors = new();
    
    public string Name
    {
        get => _name;
        set
        {
            SetProperty(ref _name, value);
            ValidateName();
        }
    }
    
    public string Email
    {
        get => _email;
        set
        {
            SetProperty(ref _email, value);
            ValidateEmail();
        }
    }
    
    public bool IsValid => !_errors.Any();
    
    // IDataErrorInfo implementation
    public string Error => string.Join(Environment.NewLine, _errors.Values);
    
    public string this[string columnName]
    {
        get
        {
            _errors.TryGetValue(columnName, out string error);
            return error;
        }
    }
    
    private void ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
            _errors[nameof(Name)] = "El nombre es requerido";
        else if (Name.Length < 2)
            _errors[nameof(Name)] = "El nombre debe tener al menos 2 caracteres";
        else
            _errors.Remove(nameof(Name));
        
        OnPropertyChanged(nameof(IsValid));
    }
    
    private void ValidateEmail()
    {
        if (string.IsNullOrWhiteSpace(Email))
            _errors[nameof(Email)] = "El email es requerido";
        else if (!IsValidEmail(Email))
            _errors[nameof(Email)] = "Formato de email inválido";
        else
            _errors.Remove(nameof(Email));
        
        OnPropertyChanged(nameof(IsValid));
    }
    
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
```

---

## 📡 Data Binding {#databinding}

### Tipos de Binding

| Tipo | Descripción | Sintaxis XAML |
|------|-------------|---------------|
| **OneWay** | Fuente → UI | `{Binding Property}` |
| **TwoWay** | Fuente ↔ UI | `{Binding Property, Mode=TwoWay}` |
| **OneTime** | Una sola vez | `{Binding Property, Mode=OneTime}` |
| **OneWayToSource** | UI → Fuente | `{Binding Property, Mode=OneWayToSource}` |

### Value Converters

```csharp
// Converter bool a visibility
[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            bool invert = parameter?.ToString().ToLower() == "invert";
            bool visible = invert ? !boolValue : boolValue;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            bool invert = parameter?.ToString().ToLower() == "invert";
            bool isVisible = visibility == Visibility.Visible;
            return invert ? !isVisible : isVisible;
        }
        return false;
    }
}

// Multi-value converter
public class MultiBindingConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values?.Length >= 2 && values[0] is string firstName && values[1] is string lastName)
        {
            return $"{firstName} {lastName}".Trim();
        }
        return string.Empty;
    }
    
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        if (value is string fullName)
        {
            var parts = fullName.Split(' ', 2);
            return new object[] { parts[0], parts.Length > 1 ? parts[1] : string.Empty };
        }
        return new object[] { string.Empty, string.Empty };
    }
}
```

### Binding Avanzado

```xml
<!-- Uso de converters -->
<TextBlock Visibility="{Binding IsDataLoaded, Converter={StaticResource BoolToVisibilityConverter}}"/>
<TextBlock Visibility="{Binding IsDataLoaded, Converter={StaticResource BoolToVisibilityConverter}, ConverterParameter=invert}"/>

<!-- MultiBinding -->
<TextBlock>
    <TextBlock.Text>
        <MultiBinding Converter="{StaticResource MultiBindingConverter}">
            <Binding Path="FirstName"/>
            <Binding Path="LastName"/>
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>

<!-- Binding con StringFormat -->
<TextBlock Text="{Binding Count, StringFormat='Total: {0} elementos'}"/>
<TextBlock Text="{Binding Price, StringFormat=C}"/>
<TextBlock Text="{Binding Date, StringFormat='dd/MM/yyyy'}"/>

<!-- Binding a recursos -->
<TextBlock Foreground="{Binding IsError, Converter={StaticResource BoolToBrushConverter}}"
           Text="{Binding Message}"/>

<!-- Binding con FallbackValue y TargetNullValue -->
<TextBlock Text="{Binding Description, FallbackValue='Sin descripción', TargetNullValue='N/A'}"/>
```

---

## 🎯 Commands {#commands}

### Command Pattern Avanzado

```csharp
// Comando con parámetro tipado
public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool> _canExecute;
    
    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }
    
    public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }
    
    public bool CanExecute(object parameter)
    {
        if (parameter is T typedParameter)
            return _canExecute?.Invoke(typedParameter) ?? true;
        return parameter == null && !typeof(T).IsValueType;
    }
    
    public void Execute(object parameter)
    {
        if (parameter is T typedParameter)
            _execute(typedParameter);
    }
}

// Command Manager personalizado
public class CommandManager
{
    private readonly Dictionary<string, ICommand> _commands = new();
    
    public void RegisterCommand(string name, ICommand command)
    {
        _commands[name] = command;
    }
    
    public ICommand GetCommand(string name)
    {
        _commands.TryGetValue(name, out ICommand command);
        return command;
    }
    
    public void ExecuteCommand(string name, object parameter = null)
    {
        var command = GetCommand(name);
        if (command?.CanExecute(parameter) == true)
        {
            command.Execute(parameter);
        }
    }
    
    public void RefreshCommands()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        });
    }
}
```

### Keyboard Shortcuts

```xml
<!-- Window con comandos de teclado -->
<Window.InputBindings>
    <KeyBinding Key="N" Modifiers="Ctrl" Command="{Binding NewCommand}"/>
    <KeyBinding Key="S" Modifiers="Ctrl" Command="{Binding SaveCommand}"/>
    <KeyBinding Key="O" Modifiers="Ctrl" Command="{Binding OpenCommand}"/>
    <KeyBinding Key="F5" Command="{Binding RefreshCommand}"/>
    <KeyBinding Key="Delete" Command="{Binding DeleteCommand}"/>
</Window.InputBindings>

<!-- Context menu con comandos -->
<DataGrid.ContextMenu>
    <ContextMenu>
        <MenuItem Header="Nuevo" Command="{Binding NewCommand}" InputGestureText="Ctrl+N"/>
        <MenuItem Header="Editar" Command="{Binding EditCommand}"/>
        <MenuItem Header="Eliminar" Command="{Binding DeleteCommand}" InputGestureText="Del"/>
        <Separator/>
        <MenuItem Header="Actualizar" Command="{Binding RefreshCommand}" InputGestureText="F5"/>
    </ContextMenu>
</DataGrid.ContextMenu>
```

---

## 🔄 Dependency Injection {#di}

### DI Container para Desktop

```csharp
// ServiceProvider para aplicaciones desktop
public class ServiceProvider
{
    private readonly Dictionary<Type, object> _singletons = new();
    private readonly Dictionary<Type, Func<object>> _transients = new();
    private readonly Dictionary<Type, Type> _mappings = new();
    
    public void RegisterSingleton<TInterface, TImplementation>()
        where TImplementation : class, TInterface
    {
        _mappings[typeof(TInterface)] = typeof(TImplementation);
    }
    
    public void RegisterSingleton<T>(T instance)
    {
        _singletons[typeof(T)] = instance;
    }
    
    public void RegisterTransient<TInterface, TImplementation>()
        where TImplementation : class, TInterface
    {
        _transients[typeof(TInterface)] = () => CreateInstance(typeof(TImplementation));
    }
    
    public T GetService<T>()
    {
        return (T)GetService(typeof(T));
    }
    
    public object GetService(Type type)
    {
        // Check singletons first
        if (_singletons.TryGetValue(type, out object singleton))
            return singleton;
        
        // Check transients
        if (_transients.TryGetValue(type, out Func<object> factory))
            return factory();
        
        // Check mappings
        if (_mappings.TryGetValue(type, out Type implementationType))
        {
            var instance = CreateInstance(implementationType);
            _singletons[type] = instance; // Cache as singleton
            return instance;
        }
        
        // Try to create directly
        return CreateInstance(type);
    }
    
    private object CreateInstance(Type type)
    {
        var constructors = type.GetConstructors();
        var constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
        
        var parameters = constructor.GetParameters()
            .Select(p => GetService(p.ParameterType))
            .ToArray();
        
        return Activator.CreateInstance(type, parameters);
    }
}

// Configuración en App.xaml.cs
public partial class App : Application
{
    private ServiceProvider _serviceProvider;
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        _serviceProvider = new ServiceProvider();
        ConfigureServices();
        
        var mainWindow = _serviceProvider.GetService<MainWindow>();
        mainWindow.Show();
    }
    
    private void ConfigureServices()
    {
        // Infrastructure
        _serviceProvider.RegisterSingleton<IDialogService, DialogService>();
        _serviceProvider.RegisterSingleton<INavigationService, NavigationService>();
        
        // Data Access
        _serviceProvider.RegisterSingleton<ICustomerRepository, CustomerRepository>();
        _serviceProvider.RegisterSingleton<IProductRepository, ProductRepository>();
        
        // Business Logic
        _serviceProvider.RegisterTransient<ICustomerService, CustomerService>();
        _serviceProvider.RegisterTransient<IProductService, ProductService>();
        
        // ViewModels
        _serviceProvider.RegisterTransient<MainWindowViewModel>();
        _serviceProvider.RegisterTransient<CustomerViewModel>();
        
        // Views
        _serviceProvider.RegisterTransient<MainWindow>();
        _serviceProvider.RegisterTransient<CustomerView>();
    }
}
```

---

## 🎪 Controles Avanzados {#controles}

### DataGrid Personalizado

```csharp
// DataGrid con funcionalidad extendida
public class ExtendedDataGrid : DataGrid
{
    public static readonly DependencyProperty AutoFilterProperty =
        DependencyProperty.Register(nameof(AutoFilter), typeof(bool), typeof(ExtendedDataGrid));
    
    public bool AutoFilter
    {
        get => (bool)GetValue(AutoFilterProperty);
        set => SetValue(AutoFilterProperty, value);
    }
    
    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        
        if (AutoFilter)
        {
            AddFilterRow();
        }
    }
    
    private void AddFilterRow()
    {
        // Implementar fila de filtros automática
        var filterRow = new DataGridRow();
        // ... configuración de filtros
    }
    
    // Exportar a Excel
    public void ExportToExcel(string fileName)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Data");
        
        // Headers
        for (int col = 0; col < Columns.Count; col++)
        {
            worksheet.Cell(1, col + 1).Value = Columns[col].Header?.ToString() ?? $"Column{col + 1}";
        }
        
        // Data
        for (int row = 0; row < Items.Count; row++)
        {
            var item = Items[row];
            for (int col = 0; col < Columns.Count; col++)
            {
                var column = Columns[col] as DataGridBoundColumn;
                var binding = column?.Binding as Binding;
                if (binding != null)
                {
                    var value = GetPropertyValue(item, binding.Path.Path);
                    worksheet.Cell(row + 2, col + 1).Value = value?.ToString() ?? "";
                }
            }
        }
        
        workbook.SaveAs(fileName);
    }
    
    private object GetPropertyValue(object obj, string propertyPath)
    {
        var properties = propertyPath.Split('.');
        object current = obj;
        
        foreach (var property in properties)
        {
            if (current == null) return null;
            current = current.GetType().GetProperty(property)?.GetValue(current);
        }
        
        return current;
    }
}
```

### TreeView con MVVM

```csharp
// TreeView item base
public abstract class TreeViewItemBase : ViewModelBase
{
    private bool _isExpanded;
    private bool _isSelected;
    private ObservableCollection<TreeViewItemBase> _children;
    
    public string Name { get; set; }
    public TreeViewItemBase Parent { get; set; }
    
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }
    
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
    
    public ObservableCollection<TreeViewItemBase> Children
    {
        get => _children ??= new ObservableCollection<TreeViewItemBase>();
        set => SetProperty(ref _children, value);
    }
    
    public void AddChild(TreeViewItemBase child)
    {
        child.Parent = this;
        Children.Add(child);
    }
    
    public void RemoveChild(TreeViewItemBase child)
    {
        child.Parent = null;
        Children.Remove(child);
    }
}

// Implementaciones específicas
public class FolderTreeItem : TreeViewItemBase
{
    public string Path { get; set; }
}

public class FileTreeItem : TreeViewItemBase
{
    public string FilePath { get; set; }
    public long Size { get; set; }
    public DateTime Modified { get; set; }
}
```

---

## 🔔 Notifications {#notifications}

### Toast Notifications

```csharp
// Servicio de notificaciones
public interface INotificationService
{
    void ShowInfo(string title, string message);
    void ShowWarning(string title, string message);
    void ShowError(string title, string message);
    void ShowSuccess(string title, string message);
}

public class NotificationService : INotificationService
{
    public void ShowInfo(string title, string message)
    {
        ShowToast(title, message, NotificationType.Info);
    }
    
    public void ShowWarning(string title, string message)
    {
        ShowToast(title, message, NotificationType.Warning);
    }
    
    public void ShowError(string title, string message)
    {
        ShowToast(title, message, NotificationType.Error);
    }
    
    public void ShowSuccess(string title, string message)
    {
        ShowToast(title, message, NotificationType.Success);
    }
    
    private void ShowToast(string title, string message, NotificationType type)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var toast = new ToastWindow(title, message, type);
            toast.Show();
        });
    }
}

// Toast Window
public partial class ToastWindow : Window
{
    private readonly Timer _timer;
    
    public ToastWindow(string title, string message, NotificationType type)
    {
        InitializeComponent();
        
        TitleText.Text = title;
        MessageText.Text = message;
        SetNotificationType(type);
        
        // Auto-close after 5 seconds
        _timer = new Timer(5000);
        _timer.Elapsed += (s, e) => Dispatcher.Invoke(Close);
        _timer.Start();
        
        // Position at bottom-right of screen
        PositionWindow();
    }
    
    private void SetNotificationType(NotificationType type)
    {
        var brush = type switch
        {
            NotificationType.Info => new SolidColorBrush(Colors.Blue),
            NotificationType.Warning => new SolidColorBrush(Colors.Orange),
            NotificationType.Error => new SolidColorBrush(Colors.Red),
            NotificationType.Success => new SolidColorBrush(Colors.Green),
            _ => new SolidColorBrush(Colors.Gray)
        };
        
        BorderBrush = brush;
        IconText.Foreground = brush;
        IconText.Text = type switch
        {
            NotificationType.Info => "ℹ",
            NotificationType.Warning => "⚠",
            NotificationType.Error => "❌",
            NotificationType.Success => "✅",
            _ => "📋"
        };
    }
    
    private void PositionWindow()
    {
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width - 20;
        Top = workArea.Bottom - Height - 20;
    }
}

public enum NotificationType
{
    Info,
    Warning,
    Error,
    Success
}
```

### System Tray Integration

```csharp
// System Tray Manager
public class SystemTrayManager : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;
    
    public SystemTrayManager()
    {
        _notifyIcon = new NotifyIcon();
        _contextMenu = new ContextMenuStrip();
        
        SetupTrayIcon();
        SetupContextMenu();
    }
    
    private void SetupTrayIcon()
    {
        _notifyIcon.Icon = SystemIcons.Application;
        _notifyIcon.Text = "Mi Aplicación WPF";
        _notifyIcon.Visible = true;
        
        _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();
        _notifyIcon.ContextMenuStrip = _contextMenu;
    }
    
    private void SetupContextMenu()
    {
        _contextMenu.Items.Add("Mostrar", null, (s, e) => ShowMainWindow());
        _contextMenu.Items.Add("Configuración", null, (s, e) => ShowSettings());
        _contextMenu.Items.Add("-");
        _contextMenu.Items.Add("Salir", null, (s, e) => ExitApplication());
    }
    
    public void ShowBalloonTip(string title, string text, ToolTipIcon icon = ToolTipIcon.Info)
    {
        _notifyIcon.ShowBalloonTip(3000, title, text, icon);
    }
    
    private void ShowMainWindow()
    {
        var mainWindow = Application.Current.MainWindow;
        if (mainWindow != null)
        {
            mainWindow.Show();
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();
        }
    }
    
    private void ShowSettings()
    {
        // Mostrar ventana de configuración
    }
    
    private void ExitApplication()
    {
        Application.Current.Shutdown();
    }
    
    public void Dispose()
    {
        _notifyIcon?.Dispose();
        _contextMenu?.Dispose();
    }
}
```

---

## 📦 Deployment {#deployment}

### Click-Once Deployment

```xml
<!-- En el archivo .csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <PublishUrl>.\publish\</PublishUrl>
    <Install>true</Install>
    <InstallFrom>Web</InstallFrom>
    <UpdateEnabled>true</UpdateEnabled>
    <UpdateMode>Foreground</UpdateMode>
    <UpdateInterval>7</UpdateInterval>
    <UpdateIntervalUnits>Days</UpdateIntervalUnits>
    <UpdatePeriodically>false</UpdatePeriodically>
    <UpdateRequired>false</UpdateRequired>
    <MapFileExtensions>true</MapFileExtensions>
    <ApplicationRevision>0</ApplicationRevision>
    <ApplicationVersion>1.0.0.%2a</ApplicationVersion>
    <UseApplicationTrust>false</UseApplicationTrust>
    <BootstrapperEnabled>true</BootstrapperEnabled>
  </PropertyGroup>
</Project>
```

### MSIX Packaging

```xml
<!-- Package.appxmanifest -->
<?xml version="1.0" encoding="utf-8"?>
<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10"
         xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10"
         xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities">
  
  <Identity Name="MyCompany.MyWPFApp"
            Publisher="CN=MyCompany"
            Version="1.0.0.0"/>
  
  <Properties>
    <DisplayName>Mi Aplicación WPF</DisplayName>
    <PublisherDisplayName>Mi Compañía</PublisherDisplayName>
    <Logo>Images\StoreLogo.png</Logo>
    <Description>Descripción de mi aplicación WPF</Description>
  </Properties>
  
  <Dependencies>
    <TargetDeviceFamily Name="Windows.Desktop" MinVersion="10.0.17763.0" MaxVersionTested="10.0.19041.0"/>
  </Dependencies>
  
  <Capabilities>
    <rescap:Capability Name="runFullTrust"/>
  </Capabilities>
  
  <Applications>
    <Application Id="App" Executable="MyWPFApp.exe" EntryPoint="Windows.FullTrustApplication">
      <uap:VisualElements DisplayName="Mi Aplicación WPF"
                          Square150x150Logo="Images\Square150x150Logo.png"
                          Square44x44Logo="Images\Square44x44Logo.png"
                          Description="Mi aplicación WPF moderna"
                          BackgroundColor="transparent">
        <uap:DefaultTile Wide310x150Logo="Images\Wide310x150Logo.png"/>
      </uap:VisualElements>
    </Application>
  </Applications>
</Package>
```

### PowerShell Deployment Script

```powershell
# Deploy-WPFApp.ps1
param(
    [Parameter(Mandatory=$true)]
    [string]$BuildConfiguration = "Release",
    
    [Parameter(Mandatory=$true)]
    [string]$OutputPath = ".\publish",
    
    [switch]$CreateInstaller,
    [switch]$SignBinaries
)

Write-Host "🚀 Iniciando deployment de aplicación WPF..." -ForegroundColor Green

# Limpiar directorios
Write-Host "🧹 Limpiando directorios..." -ForegroundColor Yellow
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
}

# Restaurar paquetes
Write-Host "📦 Restaurando paquetes NuGet..." -ForegroundColor Yellow
dotnet restore

# Compilar aplicación
Write-Host "🔨 Compilando aplicación..." -ForegroundColor Yellow
dotnet build --configuration $BuildConfiguration --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Error "❌ Error en la compilación"
    exit 1
}

# Publicar aplicación
Write-Host "📤 Publicando aplicación..." -ForegroundColor Yellow
dotnet publish --configuration $BuildConfiguration --output $OutputPath --no-build

# Firmar binarios si se solicita
if ($SignBinaries) {
    Write-Host "🔐 Firmando binarios..." -ForegroundColor Yellow
    $exeFiles = Get-ChildItem -Path $OutputPath -Filter "*.exe" -Recurse
    foreach ($exe in $exeFiles) {
        & "signtool.exe" sign /fd SHA256 /t http://timestamp.digicert.com $exe.FullName
    }
}

# Crear instalador si se solicita
if ($CreateInstaller) {
    Write-Host "📦 Creando instalador..." -ForegroundColor Yellow
    
    # Usar WiX o Inno Setup
    $issFile = ".\Setup\setup.iss"
    if (Test-Path $issFile) {
        & "ISCC.exe" $issFile
    }
}

Write-Host "✅ Deployment completado exitosamente!" -ForegroundColor Green
Write-Host "📁 Archivos publicados en: $OutputPath" -ForegroundColor Cyan
```

---

## 🔧 Best Practices

### Performance Optimization

| Técnica | Descripción | Implementación |
|---------|-------------|----------------|
| **Virtualization** | UI Virtualization para listas grandes | `VirtualizingPanel.IsVirtualizing="True"` |
| **Data Templates** | Reutilizar templates | Resource Dictionaries |
| **Weak References** | Evitar memory leaks | WeakEventManager |
| **Background Threading** | Operaciones async | Task.Run, BackgroundWorker |

### Security Considerations

```csharp
// Validación de entrada
public class SecureInputValidator
{
    public static bool ValidateInput(string input, InputType type)
    {
        return type switch
        {
            InputType.Email => IsValidEmail(input),
            InputType.Phone => IsValidPhone(input),
            InputType.SqlSafe => IsSqlSafe(input),
            _ => false
        };
    }
    
    private static bool IsSqlSafe(string input)
    {
        // Prevenir SQL injection
        var dangerous = new[] { "'", "\"", ";", "--", "/*", "*/" };
        return !dangerous.Any(input.Contains);
    }
}

// Configuración segura
public class SecureConfiguration
{
    public static void ConfigureApp()
    {
        // Deshabilitar automatic binding redirect para assemblies no confiables
        AppDomain.CurrentDomain.AssemblyResolve += (s, e) => 
        {
            // Validar assemblies antes de cargar
            return null;
        };
    }
}
```

Este documento proporciona una guía completa para el desarrollo de aplicaciones de escritorio Windows usando WinForms, WPF y WinUI 3, con énfasis en el patrón MVVM y las mejores prácticas modernas.
