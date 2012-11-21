﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Samba.Domain.Models.Accounts;
using Samba.Domain.Models.Automation;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Resources;
using Samba.Domain.Models.Settings;
using Samba.Domain.Models.Tasks;
using Samba.Domain.Models.Tickets;
using Samba.Persistance.DaoClasses;
using Samba.Persistance.Data;
using Samba.Presentation.Services.Common;
using Samba.Services;

namespace Samba.Presentation.Services.Implementations
{
    [Export(typeof(IPresentationCacheService))]
    class PresentationCacheService : AbstractService, IPresentationCacheService
    {
        private readonly IApplicationState _applicationState;
        private readonly ICacheDao _dataService;
        private readonly ICacheService _cacheService;

        [ImportingConstructor]
        public PresentationCacheService(IApplicationState applicationState, ICacheDao dataService, ICacheService cacheService)
        {
            _applicationState = applicationState;
            _dataService = dataService;
            _cacheService = cacheService;
        }

        public MenuItem GetMenuItem(Func<MenuItem, bool> expression)
        {
            return _cacheService.GetMenuItem(expression);
        }

        public IEnumerable<MenuItemPortion> GetMenuItemPortions(int menuItemId)
        {
            return GetMenuItem(x => x.Id == menuItemId).Portions;
        }

        public ProductTimer GetProductTimer(int menuItemId)
        {
            return _cacheService.GetProductTimer(_applicationState.CurrentTicketType.Id,
                                                 _applicationState.CurrentTerminal.Id,
                                                 _applicationState.CurrentDepartment.Id,
                                                 _applicationState.CurrentLoggedInUser.UserRole.Id,
                                                 menuItemId);
        }

        public IEnumerable<OrderTagGroup> GetOrderTagGroups(params int[] menuItemIds)
        {
            return _cacheService.GetOrderTagGroups(_applicationState.CurrentTicketType.Id,
                                                   _applicationState.CurrentTerminal.Id,
                                                   _applicationState.CurrentDepartment.Id,
                                                   _applicationState.CurrentLoggedInUser.UserRole.Id,
                                                   menuItemIds);
        }

        public IEnumerable<OrderStateGroup> GetOrderStateGroups(params int[] menuItemIds)
        {
            return _cacheService.GetOrderStateGroups(_applicationState.CurrentTicketType.Id,
                                                     _applicationState.CurrentTerminal.Id,
                                                     _applicationState.CurrentDepartment.Id,
                                                     _applicationState.CurrentLoggedInUser.UserRole.Id,
                                                     menuItemIds);
        }


        private IEnumerable<Resource> _resources;
        public IEnumerable<Resource> Resources
        {
            get { return _resources ?? (_resources = _dataService.GetResources()); }
        }

        public IEnumerable<Resource> GetResourcesByTemplateId(int templateId)
        {
            return Resources.Where(x => x.ResourceTypeId == templateId);
        }

        private IEnumerable<ResourceType> _resourceTypes;
        public IEnumerable<ResourceType> ResourceTypes
        {
            get { return _resourceTypes ?? (_resourceTypes = _dataService.GetResourceTypes()); }
        }

        private IEnumerable<AccountType> _accountTypes;
        public IEnumerable<AccountType> AccountTypes
        {
            get { return _accountTypes ?? (_accountTypes = _dataService.GetAccountTypes()); }
        }

        public IEnumerable<ResourceType> GetResourceTypes()
        {
            return ResourceTypes;
        }

        public ResourceType GetResourceTypeById(int resourceTypeId)
        {
            return ResourceTypes.Single(x => x.Id == resourceTypeId);
        }

        public AccountType GetAccountTypeById(int accountTypeId)
        {
            return AccountTypes.Single(x => x.Id == accountTypeId);
        }

        public Account GetAccountById(int accountId)
        {
            return Dao.SingleWithCache<Account>(x => x.Id == accountId);
        }

        public Resource GetResourceById(int accountId)
        {
            return Dao.SingleWithCache<Resource>(x => x.Id == accountId);
        }

        public IEnumerable<AccountTransactionDocumentType> GetAccountTransactionDocumentTypes(int accountTypeId)
        {
            return _cacheService.GetAccountTransactionDocumentTypes(accountTypeId,
                                                                    _applicationState.CurrentTerminal.Id,
                                                                    _applicationState.CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<AccountTransactionDocumentType> GetBatchDocumentTypes(IEnumerable<string> accountTypeNamesList)
        {
            var ids = GetAccountTypesByName(accountTypeNamesList).Select(x => x.Id);
            return _cacheService.GetBatchDocumentTypes(ids, _applicationState.CurrentTerminal.Id,
                                                       _applicationState.CurrentLoggedInUser.UserRole.Id);
        }

        private IEnumerable<ResourceState> _resourceStates;
        public IEnumerable<ResourceState> ResourceStates
        {
            get { return _resourceStates ?? (_resourceStates = _dataService.GetResourceStates()); }
        }

        public ResourceState GetResourceStateById(int accountStateId)
        {
            return ResourceStates.SingleOrDefault(x => x.Id == accountStateId);
        }

        public ResourceState GetResourceStateByName(string stateName)
        {
            return ResourceStates.FirstOrDefault(x => x.Name == stateName);
        }

        public IEnumerable<ResourceState> GetResourceStates()
        {
            return ResourceStates;
        }

        private IEnumerable<PrintJob> _printJobs;
        public IEnumerable<PrintJob> PrintJobs
        {
            get { return _printJobs ?? (_printJobs = _dataService.GetPrintJobs()); }
        }

        public PrintJob GetPrintJobByName(string name)
        {
            return PrintJobs.SingleOrDefault(x => x.Name == name);
        }

        public IEnumerable<PaymentType> GetUnderTicketPaymentTypes()
        {
            return _cacheService.GetUnderTicketPaymentTypes(_applicationState.CurrentTicketType.Id,
                                                            _applicationState.CurrentTerminal.Id,
                                                            _applicationState.CurrentDepartment.Id,
                                                            _applicationState.CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<PaymentType> GetPaymentScreenPaymentTypes()
        {
            return _cacheService.GetPaymentScreenPaymentTypes(_applicationState.CurrentTicketType.Id,
                                                            _applicationState.CurrentTerminal.Id,
                                                            _applicationState.CurrentDepartment.Id,
                                                            _applicationState.CurrentLoggedInUser.UserRole.Id);
        }

        private IEnumerable<ChangePaymentType> _changePaymentTypes;
        public IEnumerable<ChangePaymentType> ChangePaymentTypes
        {
            get { return _changePaymentTypes ?? (_changePaymentTypes = _dataService.GetChangePaymentTypes()); }
        }

        public IEnumerable<ChangePaymentType> GetChangePaymentTypes()
        {
            var maps = ChangePaymentTypes.SelectMany(x => x.ChangePaymentTypeMaps)
                .Where(x => x.TicketTypeId == 0 || x.TicketTypeId == _applicationState.CurrentTicketType.Id)
                .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == _applicationState.CurrentDepartment.Id)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id);
            return ChangePaymentTypes.Where(x => maps.Any(y => y.ChangePaymentTypeId == x.Id)).OrderBy(x => x.Order);
        }


        public IEnumerable<TicketTagGroup> GetTicketTagGroups()
        {
            return _cacheService.GetTicketTagGroups(_applicationState.CurrentTicketType.Id,
                                                    _applicationState.CurrentTerminal.Id,
                                                    _applicationState.CurrentDepartment.Id,
                                                    _applicationState.CurrentLoggedInUser.UserRole.Id);
        }

        public TicketTagGroup GetTicketTagGroupById(int id)
        {
            return _cacheService.GetTicketTagGroupById(id);
        }

        private IEnumerable<AutomationCommand> _automationCommands;
        public IEnumerable<AutomationCommand> AutomationCommands
        {
            get { return _automationCommands ?? (_automationCommands = _dataService.GetAutomationCommands()); }
        }

        public IEnumerable<AutomationCommandData> GetAutomationCommands()
        {
            var currentDepartmentId = _applicationState.CurrentDepartment != null
                                          ? _applicationState.CurrentDepartment.Id
                                          : -1;
            var maps = AutomationCommands.SelectMany(x => x.AutomationCommandMaps)
                .Where(x => x.TicketTypeId == 0 || x.TicketTypeId == _applicationState.CurrentTicketType.Id)
                .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == currentDepartmentId)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id);
            var result = maps.Select(x => new AutomationCommandData { AutomationCommand = AutomationCommands.First(y => y.Id == x.AutomationCommandId), DisplayOnPayment = x.DisplayOnPayment, DisplayOnTicket = x.DisplayOnTicket, DisplayOnOrders = x.DisplayOnOrders, VisualBehaviour = x.VisualBehaviour });
            return result.OrderBy(x => x.AutomationCommand.Order);
        }

        private IEnumerable<CalculationSelector> _calculationSelectors;
        public IEnumerable<CalculationSelector> CalculationSelectors
        {
            get { return _calculationSelectors ?? (_calculationSelectors = _dataService.GetCalculationSelectors()); }
        }

        public IEnumerable<CalculationSelector> GetCalculationSelectors()
        {
            var maps = CalculationSelectors.SelectMany(x => x.CalculationSelectorMaps)
                .Where(x => x.TicketTypeId == 0 || x.TicketTypeId == _applicationState.CurrentTicketType.Id)
                .Where(x => x.TerminalId == 0 || x.TerminalId == _applicationState.CurrentTerminal.Id)
                .Where(x => x.DepartmentId == 0 || x.DepartmentId == _applicationState.CurrentDepartment.Id)
                .Where(x => x.UserRoleId == 0 || x.UserRoleId == _applicationState.CurrentLoggedInUser.UserRole.Id);
            return CalculationSelectors.Where(x => maps.Any(y => y.CalculationSelectorId == x.Id)).OrderBy(x => x.Order);
        }

        public IEnumerable<AccountType> GetAccountTypes()
        {
            return AccountTypes;
        }

        public ChangePaymentType GetChangePaymentTypeById(int id)
        {
            return ChangePaymentTypes.Single(x => x.Id == id);
        }

        public int GetResourceTypeIdByEntityName(string entityName)
        {
            var rt = ResourceTypes.FirstOrDefault(x => x.EntityName == entityName);
            return rt != null ? rt.Id : 0;
        }

        public IEnumerable<AccountType> GetAccountTypesByName(IEnumerable<string> accountTypeNames)
        {
            return AccountTypes.Where(x => accountTypeNames.Contains(x.Name));
        }

        public MenuItemPortion GetMenuItemPortion(int menuItemId, string portionName)
        {
            var mi = GetMenuItem(x => x.Id == menuItemId);
            if (mi.Portions.Count == 0) return null;
            return mi.Portions.FirstOrDefault(x => x.Name == portionName) ?? mi.Portions[0];
        }

        public IEnumerable<ResourceScreen> GetResourceScreens()
        {
            return _cacheService.GetResourceScreens(_applicationState.CurrentTerminal.Id,
                                                    _applicationState.CurrentDepartment.Id,
                                                    _applicationState.CurrentLoggedInUser.UserRole.Id);
        }

        public IEnumerable<ResourceScreen> GetTicketResourceScreens()
        {
            return
                _cacheService.GetTicketResourceScreens(
                    _applicationState.CurrentTicketType != null ? _applicationState.CurrentTicketType.Id : 0,
                    _applicationState.CurrentTerminal.Id,
                    _applicationState.CurrentDepartment.Id,
                    _applicationState.CurrentLoggedInUser.UserRole.Id);
        }

        private IEnumerable<TicketType> _ticketTypes;
        public IEnumerable<TicketType> TicketTypes
        {
            get { return _ticketTypes ?? (_ticketTypes = _dataService.GetTicketTypes()); }
        }

        public TicketType GetTicketTypeById(int ticketTypeId)
        {
            return TicketTypes.SingleOrDefault(x => x.Id == ticketTypeId);
        }

        public IEnumerable<TicketType> GetTicketTypes()
        {
            return TicketTypes;
        }

        private IEnumerable<TaskType> _taskTypes;
        public IEnumerable<TaskType> TaskTypes
        {
            get { return _taskTypes ?? (_taskTypes = _dataService.GetTaskTypes()); }
        }

        public int GetTaskTypeIdByName(string taskTypeName)
        {
            var taskType = TaskTypes.FirstOrDefault(x => x.Name == taskTypeName);
            return taskType != null ? taskType.Id : 0;
        }

        public IEnumerable<string> GetTaskTypeNames()
        {
            return TaskTypes.Select(x => x.Name);
        }

        public void ResetOrderTagCache()
        {
            _cacheService.ResetOrderTagCache();
        }

        public void ResetTicketTagCache()
        {
            _cacheService.ResetTicketTagCache();
        }

        public override void Reset()
        {
            _cacheService.ResetCache();
            _taskTypes = null;
            _ticketTypes = null;
            _calculationSelectors = null;
            _automationCommands = null;
            _resourceTypes = null;
            _accountTypes = null;
            _resources = null;
            _resourceStates = null;
            _printJobs = null;
            _changePaymentTypes = null;
            _dataService.ResetCache();
        }
    }
}
