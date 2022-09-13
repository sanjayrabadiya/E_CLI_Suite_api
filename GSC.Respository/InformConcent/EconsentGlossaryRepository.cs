using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Shared.JWTAuth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EJ2WordDocument = Syncfusion.EJ2.DocumentEditor.WordDocument;

namespace GSC.Respository.InformConcent
{
    public class EconsentGlossaryRepository : GenericRespository<EconsentGlossary>, IEconsentGlossaryRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IEconsentSetupRepository _econsentSetupRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public EconsentGlossaryRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IEconsentSetupRepository econsentSetupRepository,
            IUploadSettingRepository uploadSettingRepository,
            IUnitOfWork uow,
            IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _econsentSetupRepository = econsentSetupRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _uow = uow;
            _mapper = mapper;
        }

        public List<DropDownDto> GetEconsentDocumentWordDropDown(int documentId)
        {
            var document = _econsentSetupRepository.Find(documentId);
            var upload = _uploadSettingRepository.GetDocumentPath();
            var FullPath = System.IO.Path.Combine(upload, document.DocumentPath);
            string path = FullPath;
            List<DropDownDto> wordList = new List<DropDownDto>();
            if (System.IO.File.Exists(path))
            {

                char[] delims = { '.', '!', '?', ',', '(', ')', '\t', '\n', '\r', ' ' };
                
                Stream stream = System.IO.File.OpenRead(path);
                WordDocument documasdasdent = new WordDocument(stream, Syncfusion.DocIO.FormatType.Doc);
                string text = documasdasdent.GetText();

                string[] words = text.Split(delims, StringSplitOptions.RemoveEmptyEntries);
                words = words.Distinct().ToArray();

                var wordId = 1;
                foreach (string word in words)
                {
                    DropDownDto item = new DropDownDto();
                    item.Id = wordId;
                    item.Value = word.Trim();
                    wordList.Add(item);
                    wordId++;
                }

            }
            return wordList;
        }

        public IList<EconsentGlossaryGridDto> GetGlossaryList(bool isDeleted, int EconsentSetupId)
        {
            var sectionrefrence = All.Where(x => x.EconsentSetupId == EconsentSetupId && (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).
                 ProjectTo<EconsentGlossaryGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            return sectionrefrence;
        }
    }
}
