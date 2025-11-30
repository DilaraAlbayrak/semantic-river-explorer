using Dapper;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;

public class GeometryHandler : SqlMapper.TypeHandler<Geometry>
{
    private readonly WKTReader _reader = new WKTReader();

    public override Geometry Parse(object value)
    {
        return _reader.Read(value.ToString());
    }

    public override void SetValue(System.Data.IDbDataParameter parameter, Geometry value)
    {
        parameter.Value = value.AsText();
    }
}