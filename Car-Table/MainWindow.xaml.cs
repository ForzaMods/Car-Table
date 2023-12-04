using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Memory;

namespace Car_Table;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public readonly Mem Mem = new()
    {
        SigScanTasks = Environment.ProcessorCount * 5
    };
    public AntiAntiCheat? AntiAntiCheat;
    private readonly Addresses? _addresses;
    public static MainWindow? Window;
    
    public MainWindow()
    {
        InitializeComponent();
        Window = this;
        _addresses = new Addresses(this);
        Loaded += (_, _) => Task.Run(_addresses.Attach);
    }


    private void CloseWindow(object sender, MouseButtonEventArgs e)
    {
        Close();
    }

    private void Window_OnClosing(object? sender, CancelEventArgs e)
    {
        AllCars.IsOn = false;
        RareCars.IsOn = false;
        FreeCars.IsOn = false;
        AntiAntiCheat?.Install();
    }

    private void MoveWindow(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void Switch_OnToggled(object sender, RoutedEventArgs e)
    {
        var isOn = (bool)sender.GetType().GetProperty("IsOn")!.GetValue(sender)!;
        sender.GetType().GetProperty("IsEnabled")?.SetValue(sender, false);
        
        switch ((string)sender.GetType().GetProperty("Name")!.GetValue(sender)!)
        {
            case "AllCars" when isOn:
            {
                RareCars.IsEnabled = false;
                
                _addresses?.Query("CREATE TABLE AutoshowTable(Id INT, NotAvailableInAutoshow INT); INSERT INTO AutoshowTable SELECT Id, NotAvailableInAutoshow FROM Data_Car; UPDATE Data_Car SET NotAvailableInAutoshow = 0;");
                break;
            }
            
            // credits to ucxh/hailstalin for sql query
            case "RareCars" when isOn:
            {
                AllCars.IsEnabled = false;
                
                _addresses?.Query("CREATE TABLE AutoshowTable(Id INT, NotAvailableInAutoshow INT); INSERT INTO AutoshowTable SELECT Id, NotAvailableInAutoshow FROM Data_Car; UPDATE Data_Car SET NotAvailableInAutoshow = (NotAvailableInAutoshow-1)* -1;");
                break;
            }
            
            case "RareCars" when !isOn:
            case "AllCars" when !isOn:
            {
                AllCars.IsEnabled = true;
                RareCars.IsEnabled = true;
                _addresses?.Query("UPDATE Data_Car SET NotAvailableInAutoshow = (SELECT NotAvailableInAutoshow FROM AutoshowTable WHERE Data_Car.Id == AutoshowTable.Id); DROP TABLE AutoshowTable;");
                break;
            }
            
            case "FreeCars" when isOn:
            {
                _addresses?.Query("CREATE TABLE CostTable(Id INT, BaseCost INT); INSERT INTO CostTable(Id, BaseCost) SELECT Id, BaseCost FROM Data_car; UPDATE Data_Car SET BaseCost = 0;");
                break;
            }
            
            case "FreeCars" when !isOn:
            {
                _addresses?.Query("UPDATE Data_Car SET BaseCost = (SELECT BaseCost FROM CostTable WHERE Id = Data_Car.Id); DROP TABLE CostTable;");
                break;
            }

            case "Perf" when isOn:
            {
                _addresses?.Query("UPDATE List_UpgradeAntiSwayFront SET price=0;UPDATE List_UpgradeAntiSwayRear SET price=0;UPDATE List_UpgradeBrakes SET price=0;UPDATE List_UpgradeCarBodyChassisStiffness SET price=0;UPDATE List_UpgradeCarBody SET price=0;UPDATE List_UpgradeCarBodyTireWidthFront SET price=0;UPDATE List_UpgradeCarBodyTireWidthRear SET price=0;UPDATE List_UpgradeCarBodyTrackSpacingFront SET price=0;UPDATE List_UpgradeCarBodyTrackSpacingRear SET price=0;UPDATE List_UpgradeCarBodyWeight SET price=0;UPDATE List_UpgradeDrivetrain SET price=0;UPDATE List_UpgradeDrivetrainClutch SET price=0;UPDATE List_UpgradeDrivetrainDifferential  SET price=0;UPDATE List_UpgradeDrivetrainDriveline SET price=0;UPDATE List_UpgradeDrivetrainTransmission SET price=0;UPDATE List_UpgradeEngine SET price=0;UPDATE List_UpgradeEngineCamshaft SET price=0;UPDATE List_UpgradeEngineCSC SET price=0;UPDATE List_UpgradeEngineDisplacement SET price=0;UPDATE List_UpgradeEngineDSC SET price=0;UPDATE List_UpgradeEngineExhaust SET price=0;UPDATE List_UpgradeEngineFlywheel SET price=0;UPDATE List_UpgradeEngineFuelSystem SET price=0;UPDATE List_UpgradeEngineIgnition SET price=0;UPDATE List_UpgradeEngineIntake SET price=0;UPDATE List_UpgradeEngineIntercooler SET price=0;UPDATE List_UpgradeEngineManifold SET price=0;UPDATE List_UpgradeEngineOilCooling SET price=0;UPDATE List_UpgradeEnginePistonsCompression SET price=0;UPDATE List_UpgradeEngineRestrictorPlate SET price=0;UPDATE List_UpgradeEngineTurboQuad SET price=0;UPDATE List_UpgradeEngineTurboSingle SET price=0;UPDATE List_UpgradeEngineTurboTwin SET price=0;UPDATE List_UpgradeEngineValves SET price=0;UPDATE List_UpgradeMotor SET price=0;UPDATE List_UpgradeMotorParts SET price=0;UPDATE List_UpgradeSpringDamper SET price=0;UPDATE List_UpgradeTireCompound SET price=0;UPDATE List_Wheels SET price=1;");
                break;
            }

            case "Visual" when isOn:
            {
                _addresses?.Query("UPDATE List_UpgradeCarBody SET price=0;UPDATE List_UpgradeCarBodyFrontBumper SET price=0;UPDATE List_UpgradeCarBodyHood SET price=0;UPDATE List_UpgradeCarBodyRearBumper SET price=0;UPDATE List_UpgradeCarBodySideSkirt SET price=0;UPDATE List_UpgradeRearWing SET price=0;UPDATE List_Wheels SET price=1;");
                break;
            }
        }
        
        sender.GetType().GetProperty("IsEnabled")?.SetValue(sender, true);
    }

    private void Discord_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        Process.Start("explorer.exe", "https://discord.gg/forzamods");
    }

    private void QuickAdd_OnClick(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show("Warning", "U can only use this with carpass, continue?", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
            return;
        }

        sender.GetType().GetProperty("IsEnabled")?.SetValue(sender, false);
        
        switch ((string)sender.GetType().GetProperty("Name")!.GetValue(sender)!)
        {
            case "AddAll":
            {
                    _addresses?.Query("INSERT INTO ContentOffersMapping (OfferId, ContentId, ContentType, IsPromo, IsAutoRedeem, ReleaseDateUTC, Quantity) SELECT 3, Id, 1, 0, 1, NULL, 1 FROM Data_Car WHERE Id NOT IN (SELECT ContentId AS Id FROM ContentOffersMapping WHERE ContentId IS NOT NULL);" +
                                " INSERT INTO Profile0_FreeCars SELECT ContentId, 1 FROM ContentOffersMapping;" +
                                " UPDATE ContentOffersMapping SET IsAutoRedeem = 1 WHERE ContentId NOT IN(SELECT ContentId FROM ContentOffersMapping WHERE ReleaseDateUTC > '" + DateTime.Now.Year + "-" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Day.ToString("00") + " 00:00');" +
                                " UPDATE ContentOffersMapping SET Quantity = 1;" +
                                " UPDATE ContentOffersMapping SET IsAutoRedeem = 0 WHERE ContentId IN(SELECT CarId AS ContentId FROM Profile0_Career_Garage WHERE CarId IS NOT NULL);");
                break;
            }
            
            case "AddRare":
            {
                _addresses?.Query("INSERT INTO ContentOffersMapping (OfferId, ContentId, ContentType, IsPromo, IsAutoRedeem, ReleaseDateUTC, Quantity) SELECT 3, Id, 1, 0, 1, NULL, 1 FROM Data_Car WHERE Id NOT IN (SELECT ContentId AS Id FROM ContentOffersMapping WHERE ContentId IS NOT NULL);" +
                                 " INSERT INTO Profile0_FreeCars SELECT Id, 1 FROM Data_Car WHERE Id NOT IN (SELECT CarId AS Id FROM Profile0_FreeCars WHERE CarID IS NOT NULL);" +
                                 " UPDATE ContentOffersMapping SET Quantity = 9999 ;" +
                                 " UPDATE Profile0_FreeCars SET FreeCount = 1;" +
                                 " UPDATE ContentOffersMapping SET IsAutoRedeem = 1;" +
                                 " UPDATE ContentOffersMapping SET IsAutoRedeem = 0 WHERE ContentId IN(SELECT Id AS ContentId FROM Data_Car WHERE NotAvailableInAutoshow = 0);" +
                                 " UPDATE ContentOffersMapping SET IsAutoRedeem = 0 WHERE ContentId IN(SELECT ContentId FROM ContentOffersMapping WHERE ReleaseDateUTC > '" + DateTime.Now.Year + "-" + DateTime.Now.Month.ToString("00") + "-" + DateTime.Now.Day.ToString("00") + " 00:00');");
                break;
            }
        }
        
        sender.GetType().GetProperty("IsEnabled")?.SetValue(sender, true);
    }

    private void GarageMods_OnClick(object sender, RoutedEventArgs e)
    {
        sender.GetType().GetProperty("IsEnabled")?.SetValue(sender, false);
        
        switch ((string)sender.GetType().GetProperty("Name")!.GetValue(sender)!)
        {
            case "Perf":
            {
                _addresses?.Query("UPDATE List_UpgradeAntiSwayFront SET Price = 0; UPDATE List_UpgradeAntiSwayRear SET Price = 0; UPDATE List_UpgradeBrakes SET Price = 0; UPDATE List_UpgradeTireCompound SET Price = 0; UPDATE List_UpgradeSpringDamper SET Price = 0; UPDATE List_UpgradeRimSizeRear SET Price = 0;UPDATE List_UpgradeRimSizeFront SET Price = 0; UPDATE List_UpgradeMotorParts set Price = 0; UPDATE List_UpgradeMotor SET Price = 0; UPDATE List_UpgradeEngineValves SET Price = 0; UPDATE List_UpgradeEngineTurboTwin SET Price = 0; UPDATE List_UpgradeEngineTurboSingle SET Price = 0;UPDATE List_UpgradeEngineTurboQuad SET Price = 0; UPDATE List_UpgradeEngineRestrictorPlate SET Price = 0; UPDATE List_UpgradeEngineOilCooling SET Price = 0; UPDATE List_UpgradeEngineManifold SET Price = 0; UPDATE List_UpgradeEngineIntercooler SET Price = 0;UPDATE List_UpgradeEngineIntake SET Price = 0; UPDATE List_UpgradeEngineIgnition SET Price = 0; UPDATE List_UpgradeEngineFuelSystem SET Price = 0; UPDATE List_UpgradeEngineFlywheel SET Price = 0; UPDATE List_UpgradeEngineExhaust SET Price = 0;UPDATE List_UpgradeEngineDisplacement SET Price = 0; UPDATE List_UpgradeEngineDSC SET Price = 0; UPDATE List_UpgradeEngineDSC SET Price = 0; UPDATE List_UpgradeEngine SET Price = 0; UPDATE List_UpgradeDrivetrainTransmission SET Price = 0; UPDATE List_UpgradeDrivetrainDriveline SET Price = 0; UPDATE List_UpgradeCarBodyTrackSpacingRear SET Price = 0; UPDATE List_UpgradeCarBodyTrackSpacingFront SET Price = 0; UPDATE List_UpgradeCarBodyTireWidthRear SET Price = 0; UPDATE List_UpgradeCarBodyTireAspectRatioFront SET Price = 0;");
                break;
            }

            case "Visual":
            {
                _addresses?.Query("UPDATE List_UpgradeCarBody SET price=0;UPDATE List_UpgradeCarBodyFrontBumper SET price=0;UPDATE List_UpgradeCarBodyHood SET price=0;UPDATE List_UpgradeCarBodyRearBumper SET price=0;UPDATE List_UpgradeCarBodySideSkirt SET price=0;UPDATE List_UpgradeRearWing SET price=0;UPDATE List_Wheels SET price=1;");
                break;
            }
        }
        
        sender.GetType().GetProperty("IsEnabled")?.SetValue(sender, true);
    }
}