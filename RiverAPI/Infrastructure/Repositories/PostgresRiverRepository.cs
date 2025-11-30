using Npgsql;
using Dapper;
using RiverAPI.Domain;

namespace RiverAPI.Infrastructure.Repositories
{
    public class PostgresRiverRepository : IRiverRepository
    {
        private readonly NpgsqlDataSource _dataSource;

        public PostgresRiverRepository(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public async Task<IEnumerable<River>> GetAllAsync(int pageNumber, int pageSize)
        {
            await using var conn = await _dataSource.OpenConnectionAsync();

            var offset = (pageNumber - 1) * pageSize;

            var sql = @"SELECT id, flow_direction, length, fictitious::boolean, form, 
                       watercourse_name, watercourse_name_alternative, 
                       start_node, end_node, geometry
                FROM rivers
                ORDER BY id
                OFFSET @offset LIMIT @pageSize";

            return await conn.QueryAsync<River>(sql, new { offset, pageSize });
        }

        public async Task<River?> GetByIdAsync(string id)
        {
            await using var conn = await _dataSource.OpenConnectionAsync();

            var sql = @"SELECT id, flow_direction, length, fictitious::boolean, form, 
                       watercourse_name, watercourse_name_alternative, 
                       start_node, end_node, geometry
                FROM rivers WHERE id = @id";

            return await conn.QuerySingleOrDefaultAsync<River>(sql, new { id });
        }

        public async Task<IEnumerable<River>> GetByBoundingBoxAsync(double minX, double minY, double maxX, double maxY)
        {
            await using var conn = await _dataSource.OpenConnectionAsync();

            var sql = @"SELECT id, flow_direction, length, fictitious::boolean, form, 
                               watercourse_name, watercourse_name_alternative, 
                               start_node, end_node, geometry
                        FROM rivers
                        WHERE ST_Intersects(
                            geometry,
                            ST_MakeEnvelope(@minX, @minY, @maxX, @maxY, 27700)
                        )";

            return await conn.QueryAsync<River>(sql, new { minX, minY, maxX, maxY });
        }
    }
}