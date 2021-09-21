using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ServiceHost.Client.Shared.ConfigurationServer.Data;
using Tauron.Application;

namespace ServiceManager.Client.Shared.Configuration.ConditionEditor
{
    public sealed class InstalledAppElement : BaseSingleElement
    {
        public InstalledAppElement()
            : base(ConditionType.InstalledApp) { }

        protected override BaseSingleElement CreateThis()
            => new InstalledAppElement();
    }

    public sealed class DefinedAppElement : BaseSingleElement
    {
        public DefinedAppElement()
            : base(ConditionType.DefinedApp) { }

        protected override BaseSingleElement CreateThis()
            => new DefinedAppElement();
    }

    public sealed class OrElement : BaseListElement
    {
        public OrElement()
            : base(ConditionType.Or, "Zu wenig Elemenete für Oder Bedingung") { }

        protected override BaseListElement CreateThis()
            => new OrElement();
    }

    public sealed class AndElement : BaseListElement
    {
        public AndElement()
            : base(ConditionType.And, "Zu wenig Elemenete für Und Bedingung") { }

        protected override BaseListElement CreateThis()
            => new AndElement();
    }

    public abstract class BaseSingleElement : ElementItem
    {
        private readonly ConditionType _conditionType;
        private string _appName = string.Empty;

        protected BaseSingleElement(ConditionType conditionType)
            => _conditionType = conditionType;

        public string AppName
        {
            get => _appName;
            set
            {
                if (value == _appName) return;

                _appName = value;
                OnPropertyChanged();
            }
        }

        public static TResult Make<TResult>(TResult newly, Condition condition)
            where TResult : BaseSingleElement
        {
            TransferSingle(newly, condition);

            return newly;
        }

        private static void TransferSingle(BaseSingleElement item, Condition condition)
        {
            TransferBasic(item, condition);
            item.AppName = condition.AppName ?? string.Empty;
        }

        protected abstract BaseSingleElement CreateThis();

        public override ElementItem Clone()
        {
            var temp = CreateThis();
            CopyTo(temp);
            temp.AppName = AppName;

            return temp;
        }

        public override string? Validate(bool single)
            => string.IsNullOrWhiteSpace(AppName)
                ? "Kein Name einer Anwendung gewählt"
                : null;

        public override Condition Build()
            => new(GetName(), Excluding, AppName, _conditionType, null, Order);
    }

    public abstract class BaseListElement : ElementItem
    {
        private readonly ConditionType _conditionType;
        private readonly string _errorMessage;

        protected BaseListElement(ConditionType conditionType, string errorMessage)
        {
            _conditionType = conditionType;
            _errorMessage = errorMessage;
        }

        public List<ElementItem> Items { get; private set; } = new();

        public static TResult Make<TResult>(TResult newly, Condition condition)
            where TResult : BaseListElement
        {
            TransferList(newly, condition);

            return newly;
        }

        private static void TransferList(BaseListElement item, Condition condition)
        {
            TransferBasic(item, condition);

            if (condition.Conditions == null) return;

            item.Items = condition.Conditions.Select(CreateItem).ToList();
        }

        protected abstract BaseListElement CreateThis();

        public override ElementItem Clone()
        {
            var temp = CreateThis();
            CopyTo(temp);
            temp.Items = Items.Select(i => i.Clone()).ToList();

            return temp;
        }

        public override string? Validate(bool single)
            => Items.Count < 2
                ? _errorMessage
                : single
                    ? null
                    : Items.Select(e => e.Validate(single)).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));

        public override Condition Build()
            => new(GetName(), Excluding, null, _conditionType, Items.Select(c => c.Build()).ToImmutableList()!, Order);
    }

    public sealed class InvalidItem : ElementItem
    {
        public override string Validate(bool single)
            => "Kein Element Vorhanden";

        public override ElementItem Clone()
            => new InvalidItem();

        public override Condition? Build()
            => null;
    }

    public abstract class ElementItem : ObservableObject
    {
        public const string IdPrefix = "ID-";

        private static readonly ElementItem Invalid = new InvalidItem();
        private bool _excluding;
        private string? _name;
        private int _order;

        public bool Excluding
        {
            get => _excluding;
            set
            {
                if (value == _excluding) return;

                _excluding = value;
                OnPropertyChanged();
            }
        }

        public int Order
        {
            get => _order;
            set
            {
                if (value == _order) return;

                _order = value;
                OnPropertyChanged();
            }
        }

        public string? Name
        {
            get => _name;
            set
            {
                if (value == _name) return;

                _name = value;
                OnPropertyChanged();
            }
        }

        protected static void TransferBasic(ElementItem item, Condition condition)
        {
            item.Name = condition.Name;
            item.Excluding = condition.Excluding;
            item.Order = condition.Order;
        }

        public static ElementItem CreateItem(Condition condition)
            => condition.Type switch
            {
                ConditionType.And => BaseListElement.Make(new AndElement(), condition),
                ConditionType.Or => BaseListElement.Make(new OrElement(), condition),
                ConditionType.InstalledApp => BaseSingleElement.Make(new InstalledAppElement(), condition),
                ConditionType.DefinedApp => BaseSingleElement.Make(new DefinedAppElement(), condition),
                _ => Invalid
            };

        public abstract string? Validate(bool single);

        protected void CopyTo(ElementItem newItem, bool icludeName = false)
        {
            newItem.Excluding = Excluding;
            if (icludeName)
                newItem.Name = Name;
            newItem.Order = Order;
        }

        public abstract ElementItem Clone();

        public abstract Condition? Build();

        protected string GetName()
            => string.IsNullOrWhiteSpace(Name)
                ? $"{IdPrefix}{Guid.NewGuid():D}"
                : Name;
    }
}