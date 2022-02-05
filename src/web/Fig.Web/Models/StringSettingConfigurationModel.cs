﻿using Fig.Contracts.SettingDefinitions;
using Fig.Web.Events;

namespace Fig.Web.Models
{
    public class StringSettingConfigurationModel : SettingConfigurationModel
    {
        public StringSettingConfigurationModel(SettingDefinitionDataContract dataContract, Action<SettingEvent> stateChanged)
            : base(dataContract, stateChanged)
        {
            Value = dataContract.Value;
            DefaultValue = dataContract.DefaultValue;
        }

        public string Value { get; set; }

        public string DefaultValue { get; set; }

        public string UpdatedValue { get; set; }

        public string ConfirmUpdatedValue { get; set; }

        public override dynamic GetValue()
        {
            return Value;
        }

        protected override void ApplyUpdatedSecretValue()
        {
            Value = UpdatedValue;
        }

        protected override bool IsUpdatedSecretValueValid()
        {
            return !string.IsNullOrWhiteSpace(UpdatedValue) &&
                    UpdatedValue == ConfirmUpdatedValue;
        }

        internal override SettingConfigurationModel Clone(Action<SettingEvent> stateChanged)
        {
            var clone = new StringSettingConfigurationModel(_definitionDataContract, stateChanged)
            {
                IsDirty = true,
            };

            return clone;
        }
    }
}
