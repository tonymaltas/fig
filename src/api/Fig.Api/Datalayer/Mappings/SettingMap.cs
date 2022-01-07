using Fig.Api.Datalayer.BusinessEntities;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Fig.Api.Datalayer.Mappings;

public class SettingMap : ClassMapping<SettingBusinessEntity>
{
    public SettingMap()
    {
        Table("setting");
        Id(x => x.Id, m => m.Generator(Generators.GuidComb));
        Property(x => x.Name, x => x.Column("name"));
        Property(x => x.Description, x => x.Column("description"));
        Property(x => x.IsSecret, x => x.Column("is_secret"));
        Property(x => x.ValidationType, x => x.Column("validation_type")); 
        Property(x => x.ValidationRegex, x => x.Column("validation_regex"));
        Property(x => x.ValidationExplanation, x => x.Column("validation_explanation"));
        Property(x => x.ValidValuesAsJson, x =>
        {
            x.Column("valid_values_json");
            x.Type(NHibernateUtil.StringClob);
        });
        Property(x => x.Group, x => x.Column("group_key"));
        Property(x => x.DisplayOrder, x => x.Column("display_order"));
        Property(x => x.Advanced, x => x.Column("advanced"));
        Property(x => x.StringFormat, x => x.Column("string_format"));
        Property(x => x.ValueType, x => x.Column("value_type"));
        Property(x => x.ValueAsJson, x =>
        {
            x.Column("value_json");
            x.Type(NHibernateUtil.StringClob);
        });
        Property(x => x.DefaultValueType, x => x.Column("default_value_type"));
        Property(x => x.DefaultValueAsJson, x =>
        {
            x.Column("default_value_json");
            x.Type(NHibernateUtil.StringClob);
        });
    }
}