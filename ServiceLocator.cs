using Optimizer.Domain.Interfaces;
using Optimizer.Domain.Services;
using Optimizer.Infrastructure.Backup;
using Optimizer.Infrastructure.Logging;
using Optimizer.Infrastructure.Registry;
using Optimizer.Infrastructure.RestorePoint;
using Optimizer.Infrastructure.Security;
using Optimizer.Infrastructure.Services;
using Optimizer.Infrastructure.SystemInfo;
using Optimizer.Infrastructure.Optimization;

namespace Optimizer
{
    /// <summary>
    /// A simple static ServiceLocator to resolve dependencies since we are not using a full DI container.
    /// This should be initialized at application startup.
    /// </summary>
    public static class ServiceLocator
    {
        public static ILoggerService LoggerService { get; private set; }
        public static IPrivilegeService PrivilegeService { get; private set; }
        public static ISystemInfoService SystemInfoService { get; private set; }
        public static IProfileOptimizationService ProfileOptimizationService { get; private set; }
        public static ISnapshotService SnapshotService { get; private set; }
        public static IRegistryService RegistryService { get; private set; }
        public static IWindowsServiceManager WindowsServiceManager { get; private set; }
        public static IRestorePointService RestorePointService { get; private set; }
        public static ISystemRestoreService SystemRestoreService { get; private set; }
        public static IRealtimeMonitorService RealtimeMonitorService { get; private set; }
        public static ISmartProfileBackupService SmartProfileBackupService { get; private set; }
        public static IHostsManagerService HostsManagerService { get; private set; }
        public static INetworkToolsService NetworkToolsService { get; private set; }
        public static IPrivacyProtectionService PrivacyProtectionService { get; private set; }
        public static ISystemOptimizationAssessmentService SystemOptimizationAssessmentService { get; private set; }
        public static IChangeBackupService ChangeBackupService { get; private set; }
        public static IChangeExecutionService ChangeExecutionService { get; private set; }
        public static IWindowsServicesReviewService WindowsServicesReviewService { get; private set; }
        public static IAppRemovalReviewService AppRemovalReviewService { get; private set; }
        public static IAdvancedUtilitiesReviewService AdvancedUtilitiesReviewService { get; private set; }

        private static readonly object _initLock = new object();
        private static bool _initialized = false;

        public static void Initialize()
        {
            lock (_initLock)
            {
                if (_initialized) return;

                // Initialize services in correct dependency order
                LoggerService = new LoggerService();
                LoggerService.Initialize();

                PrivilegeService = new PrivilegeService(LoggerService);
                RestorePointService = new RestorePointService(LoggerService);
                SystemRestoreService = new SystemRestoreService(LoggerService);
                SmartProfileBackupService = new SmartProfileBackupService(RestorePointService);
                ProfileOptimizationService = new ProfileOptimizationService(SmartProfileBackupService);
                SystemInfoService = new SystemInfoService();
                SnapshotService = new SnapshotService(LoggerService);
                RegistryService = new RegistryService(LoggerService, SnapshotService);
                WindowsServiceManager = new WindowsServiceManager(LoggerService);
                RealtimeMonitorService = new RealtimeMonitorService();
                HostsManagerService = new HostsManagerService(PrivilegeService);
                NetworkToolsService = new NetworkToolsService(LoggerService);
                PrivacyProtectionService = new PrivacyProtectionService();
                SystemOptimizationAssessmentService = new SystemOptimizationAssessmentService();
                ChangeBackupService = new ChangeBackupService();
                ChangeExecutionService = new ChangeExecutionService(ChangeBackupService);
                WindowsServicesReviewService = new WindowsServicesReviewService();
                AppRemovalReviewService = new AppRemovalReviewService();
                AdvancedUtilitiesReviewService = new AdvancedUtilitiesReviewService();

                _initialized = true;
                LoggerService.Info("ServiceLocator initialized.");
            }
        }
    }
}
