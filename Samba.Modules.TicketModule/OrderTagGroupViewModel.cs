﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.TicketModule
{
    public class OrderTagGroupViewModel : EntityViewModelBase<OrderTagGroup>
    {
        private readonly IEnumerable<Order> _selectedOrders;
        
        private ObservableCollection<OrderTagViewModel> _orderTags;
        public ObservableCollection<OrderTagViewModel> OrderTags { get { return _orderTags ?? (_orderTags = new ObservableCollection<OrderTagViewModel>(GetOrderTags(_selectedOrders, Model))); } }

        private ObservableCollection<OrderTagMapViewModel> _orderTagMaps;
        public ObservableCollection<OrderTagMapViewModel> OrderTagMaps { get { return _orderTagMaps ?? (_orderTagMaps = new ObservableCollection<OrderTagMapViewModel>(GetOrderTagMaps(Model))); } }

        private readonly IList<string> _selectionTypes = new[] { "Multiple Selection", "Single Selection", "Quantity Selection" };
        public IList<string> SelectionTypes { get { return _selectionTypes; } }

        public ICaptionCommand AddOrderTagCommand { get; set; }
        public ICaptionCommand DeleteOrderTagCommand { get; set; }
        public ICaptionCommand AddOrderTagMapCommand { get; set; }
        public ICaptionCommand DeleteOrderTagMapCommand { get; set; }

        public string ButtonHeader { get { return Model.ButtonHeader; } set { Model.ButtonHeader = value; } }
        public bool AddTagPriceToOrderPrice { get { return Model.AddTagPriceToOrderPrice; } set { Model.AddTagPriceToOrderPrice = value; } }
        public int ButtonHeight { get { return Model.ButtonHeight; } set { Model.ButtonHeight = value; } }
        public int ColumnCount { get { return Model.ColumnCount; } set { Model.ColumnCount = value; } }
        public int TerminalButtonHeight { get { return Model.TerminalButtonHeight; } set { Model.TerminalButtonHeight = value; } }
        public int TerminalColumnCount { get { return Model.TerminalColumnCount; } set { Model.TerminalColumnCount = value; } }
        public string SelectionType { get { return SelectionTypes[Model.SelectionType]; } set { Model.SelectionType = SelectionTypes.IndexOf(value); } }
        public bool UnlocksOrder { get { return Model.UnlocksOrder; } set { Model.UnlocksOrder = value; } }
        public bool CalculateOrderPrice { get { return Model.CalculateOrderPrice; } set { Model.CalculateOrderPrice = value; } }
        public bool DecreaseOrderInventory { get { return Model.DecreaseOrderInventory; } set { Model.DecreaseOrderInventory = value; } }

        public OrderTagViewModel SelectedOrderTag { get; set; }
        public OrderTagMapViewModel SelectedOrderTagMap { get; set; }

        public OrderTagGroupViewModel(OrderTagGroup model)
            : this(null, model)
        {
        }

        public OrderTagGroupViewModel(IEnumerable<Order> selectedOrders, OrderTagGroup model)
            : base(model)
        {
            _selectedOrders = selectedOrders;
            AddOrderTagCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.OrderTag), OnAddPropertyExecuted);
            DeleteOrderTagCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.OrderTag), OnDeletePropertyExecuted, CanDeleteProperty);
            AddOrderTagMapCommand = new CaptionCommand<string>(Resources.Add, OnAddOrderTagMap);
            DeleteOrderTagMapCommand = new CaptionCommand<string>(Resources.Delete, OnDeleteOrderTagMap, CanDeleteOrderTagMap);
        }

        private bool CanDeleteOrderTagMap(string arg)
        {
            return SelectedOrderTagMap != null;
        }

        private void OnDeleteOrderTagMap(string obj)
        {
            if (SelectedOrderTagMap.Id > 0)
                Workspace.Delete(SelectedOrderTagMap.Model);
            Model.OrderTagMaps.Remove(SelectedOrderTagMap.Model);
            OrderTagMaps.Remove(SelectedOrderTagMap);
        }

        private void OnAddOrderTagMap(string obj)
        {
            OrderTagMaps.Add(new OrderTagMapViewModel(Model.AddOrderTagMap()));
        }

        private void OnDeletePropertyExecuted(string obj)
        {
            if (SelectedOrderTag == null) return;
            if (SelectedOrderTag.Model.Id > 0)
                Workspace.Delete(SelectedOrderTag.Model);
            Model.OrderTags.Remove(SelectedOrderTag.Model);
            OrderTags.Remove(SelectedOrderTag);
        }

        private bool CanDeleteProperty(string arg)
        {
            return SelectedOrderTag != null;
        }

        private void OnAddPropertyExecuted(string obj)
        {
            OrderTags.Add(new OrderTagViewModel(MenuItem.AddDefaultMenuItemProperty(Model)));
        }

        private static IEnumerable<OrderTagViewModel> GetOrderTags(IEnumerable<Order> selectedOrders, OrderTagGroup baseModel)
        {
            return baseModel.OrderTags.Select(item => new OrderTagViewModel(selectedOrders, item));
        }

        private static IEnumerable<OrderTagMapViewModel> GetOrderTagMaps(OrderTagGroup model)
        {
            return model.OrderTagMaps.Select(x => new OrderTagMapViewModel(x));
        }

        public override string GetModelTypeString()
        {
            return Resources.OrderTagGroup;
        }

        public override Type GetViewType()
        {
            return typeof(OrderTagGroupView);
        }
    }
}