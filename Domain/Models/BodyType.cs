using System;

namespace Postgirl.Domain.Models;

public enum BodyType
{
    None,
    Json,
    Text,
    Xml,
    FormUrlEncoded
}
public static class BodyTypeHelper
{
    public static Array GetValues => Enum.GetValues(typeof(BodyType));
}