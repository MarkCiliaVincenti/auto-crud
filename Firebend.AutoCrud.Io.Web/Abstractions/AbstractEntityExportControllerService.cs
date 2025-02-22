using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Firebend.AutoCrud.Core.Implementations;
using Firebend.AutoCrud.Core.Interfaces;
using Firebend.AutoCrud.Core.Interfaces.Models;
using Firebend.AutoCrud.Core.Interfaces.Services.Entities;
using Firebend.AutoCrud.Io.Interfaces;
using Firebend.AutoCrud.Io.Models;
using Firebend.AutoCrud.Io.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Firebend.AutoCrud.Io.Web.Abstractions
{
    public abstract class AbstractEntityExportControllerService<TKey, TEntity, TVersion, TSearch, TMapped> :
        BaseDisposable,
        IEntityExportControllerService<TKey, TEntity, TVersion, TSearch, TMapped>
        where TSearch : IEntitySearchRequest
        where TMapped : class
        where TEntity : class, IEntity<TKey>
        where TVersion : class, IAutoCrudApiVersion
        where TKey : struct
    {
        private readonly IEntityFileTypeMimeTypeMapper<TVersion> _entityFileTypeMimeTypeMapper;
        private readonly IEntityExportService<TMapped, TVersion> _exportService;
        private readonly IEntityExportMapper<TEntity, TVersion, TMapped> _mapper;
        private readonly IEntitySearchService<TKey, TEntity, TSearch> _searchService;

        protected AbstractEntityExportControllerService(IEntityFileTypeMimeTypeMapper<TVersion> entityFileTypeMimeTypeMapper,
            IEntityExportService<TMapped, TVersion> exportService,
            IEntityExportMapper<TEntity, TVersion, TMapped> mapper,
            IEntitySearchService<TKey, TEntity, TSearch> searchService)
        {
            _entityFileTypeMimeTypeMapper = entityFileTypeMimeTypeMapper;
            _exportService = exportService;
            _mapper = mapper;
            _searchService = searchService;
        }

        public async Task<FileResult> ExportEntitiesAsync(EntityFileType fileType,
            string fileName,
            TSearch search,
            CancellationToken cancellationToken = default)
        {
            if (search == null)
            {
                throw new ArgumentNullException(nameof(search));
            }

            var entities = await _searchService.SearchAsync(search, cancellationToken);

            var records = MapRecords(entities);

            var fileStream = await _exportService.ExportAsync(fileType, records, cancellationToken);

            var mimeType = _entityFileTypeMimeTypeMapper.MapMimeType(fileType);
            var extension = _entityFileTypeMimeTypeMapper.GetExtension(fileType);

            var fileResult = new FileStreamResult(fileStream, mimeType) { FileDownloadName = $"{fileName}{extension}" };

            return fileResult;
        }

        private IEnumerable<TMapped> MapRecords(IEnumerable<TEntity> data)
        {
            if (data == null)
            {
                return Array.Empty<TMapped>();
            }

            var mappedRecords = data
                .Select(_mapper.Map)
                .ToArray();

            return mappedRecords;
        }

        protected override void DisposeManagedObjects()
        {
            _searchService?.Dispose();
            _exportService?.Dispose();
        }
    }
}
