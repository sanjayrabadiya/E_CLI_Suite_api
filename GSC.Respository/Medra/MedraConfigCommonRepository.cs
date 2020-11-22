using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.PropertyMapping;
using GSC.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.Medra
{
    public class MedraConfigCommonRepository : GenericRespository<MedraConfig, GscContext>, IMedraConfigCommonRepository
    {
        private IPropertyMappingService _propertyMappingService;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUnitOfWork<GscContext> _uow;
        public MedraConfigCommonRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser, IPropertyMappingService propertyMappingService) : base(uow, jwtTokenAccesser)
        {
            _propertyMappingService = propertyMappingService;
            _jwtTokenAccesser = jwtTokenAccesser;
            _uow = uow;
        }

        public void AddDataInMedraTableUsingAsciiFile(int MedraConfigId, string path, FolderType folderType, string Language, string Version, string Rootname)
        {
            string[] paths = { path, folderType.ToString(), Language, Version, Rootname };
            var fullPath = Path.Combine(paths) + "\\Unzip\\MedAscii";

            string[] fileEntries = Directory.GetFiles(fullPath);
            foreach (string fileName in fileEntries)
            {
                //List<_1_low_level_term> LltModel = new List<_1_low_level_term>();
                //List<_1_hlgt_hlt_comp> HhcModel = new List<_1_hlgt_hlt_comp>();
                //List<_1_hlgt_pref_term> HlptModel = new List<_1_hlgt_pref_term>();
                //List<_1_hlt_pref_comp> HpcModel = new List<_1_hlt_pref_comp>();
                //List<_1_hlt_pref_term> HptModel = new List<_1_hlt_pref_term>();
                //List<_1_md_hierarchy> MhModel = new List<_1_md_hierarchy>();
                //List<_1_pref_term> PtModel = new List<_1_pref_term>();
                //List<_1_smq_Content> ScModel = new List<_1_smq_Content>();
                //List<_1_smq_list> SlModel = new List<_1_smq_list>();
                //List<_1_soc_hlgt_comp> ShcModel = new List<_1_soc_hlgt_comp>();
                //List<_1_soc_intl_order> SioModel = new List<_1_soc_intl_order>();
                //List<_1_soc_term> StModel = new List<_1_soc_term>();

                var Lines = File.ReadAllLines(fileName);
                foreach (var line in Lines)
                {
                    var allField = line.Split('$');

                    //if (fileName == fullPath + "\\llt.asc") {
                    //    _1_low_level_term model = new _1_low_level_term();
                    //    model.MedraConfigId = MedraConfigId;
                    //    model.llt_code = Convert.ToInt64(allField[0]);
                    //    model.llt_name = allField[1];
                    //    model.pt_code = Convert.ToInt64(allField[2]);
                    //    model.llt_whoart_code = allField[3];
                    //    model.llt_harts_code = allField[4] == "" ? (long?)null : Convert.ToInt64(allField[4]);
                    //    model.llt_costart_sym = allField[5];
                    //    model.llt_icd9_code = allField[6];
                    //    model.llt_icd9cm_code = allField[7];
                    //    model.llt_icd10_code = allField[8];
                    //    model.llt_currency = allField[9];
                    //    model.llt_jart_code = allField[10];
                    //    Context._1_low_level_term.Add(model);
                        
                    //}
                    //else if (fileName == fullPath + "\\pt.asc") {
                    //    _1_pref_term model = new _1_pref_term();
                    //    model.MedraConfigId = MedraConfigId;
                    //    model.pt_code = Convert.ToInt64(allField[0]);
                    //    model.pt_name = allField[1];
                    //    model.null_field = allField[2];
                    //    model.pt_soc_code = allField[3] == "" ? (long?)null : Convert.ToInt64(allField[3]);
                    //    model.pt_whoart_code = allField[4];
                    //    model.pt_harts_code = allField[5] == "" ? (long?)null : Convert.ToInt64(allField[5]);
                    //    model.pt_costart_sym = allField[6];
                    //    model.pt_icd9_code = allField[7];
                    //    model.pt_icd9cm_code = allField[8];
                    //    model.pt_icd10_code = allField[9];
                    //    model.pt_jart_code = allField[10];
                    //    Context._1_pref_term.Add(model);
                    //}
                    //else if (fileName == fullPath + "\\hlt.asc") {
                    //    _1_hlt_pref_term model = new _1_hlt_pref_term();
                    //    model.MedraConfigId = MedraConfigId;
                    //    model.hlt_code = Convert.ToInt64(allField[0]);
                    //    model.hlt_name = allField[1];
                    //    model.hlt_whoart_code = allField[2];
                    //    model.hlt_harts_code = allField[3] == "" ? (long?)null : Convert.ToInt64(allField[3]);
                    //    model.hlt_costart_sym = allField[4];
                    //    model.hlt_icd9_code = allField[5];
                    //    model.hlt_icd9cm_code = allField[6];
                    //    model.hlt_icd10_code = allField[7];
                    //    model.hlt_jart_code = allField[8];
                    //    Context._1_hlt_pref_term.Add(model);
                    //}
                    //else if (fileName == fullPath + "\\hlt_pt.asc") {
                    //    _1_hlt_pref_comp model = new _1_hlt_pref_comp();
                    //    model.MedraConfigId = MedraConfigId;
                    //    model.hlt_code = Convert.ToInt64(allField[0]);
                    //    model.pt_code = Convert.ToInt64(allField[1]);
                    //    Context._1_hlt_pref_comp.Add(model);
                    //}
                    //else if (fileName == fullPath + "\\hlgt.asc") {
                    //    _1_hlgt_pref_term model = new _1_hlgt_pref_term();
                    //    model.MedraConfigId = MedraConfigId;
                    //    model.hlgt_code = Convert.ToInt64(allField[0]);
                    //    model.hlgt_name = allField[1];
                    //    model.hlgt_whoart_code = allField[2];
                    //    model.hlgt_harts_code = allField[3] == "" ? (long?)null : Convert.ToInt64(allField[3]);
                    //    model.hlgt_costart_sym = allField[4];
                    //    model.hlgt_icd9_code = allField[5];
                    //    model.hlgt_icd9cm_code = allField[6];
                    //    model.hlgt_icd10_code = allField[7];
                    //    model.hlgt_jart_code = allField[8];
                    //    Context._1_hlgt_pref_term.Add(model);
                    //}
                    //else if (fileName == fullPath + "\\hlgt_hlt.asc") {
                    //    _1_hlgt_hlt_comp model = new _1_hlgt_hlt_comp();
                    //    model.MedraConfigId = MedraConfigId;
                    //    model.hlgt_code = Convert.ToInt64(allField[0]);
                    //    model.hlt_code = Convert.ToInt64(allField[1]);
                    //    Context._1_hlgt_hlt_comp.Add(model);
                    //}
                    //else if (fileName == fullPath + "\\soc.asc") {
                    //    _1_soc_term model = new _1_soc_term();
                    //    model.MedraConfigId = MedraConfigId;
                    //    model.soc_code = Convert.ToInt64(allField[0]);
                    //    model.soc_name = allField[1];
                    //    model.soc_abbrev = allField[2];
                    //    model.soc_harts_code = allField[3] == "" ? (long?)null : Convert.ToInt64(allField[3]);
                    //    model.soc_costart_sym = allField[4];
                    //    model.soc_icd9_code = allField[5];
                    //    model.soc_icd9cm_code = allField[6];
                    //    model.soc_icd10_code = allField[7];
                    //    model.soc_jart_code = allField[8];
                    //    Context._1_soc_term.Add(model);
                    //}
                    //else if (fileName == fullPath + "\\soc_hlgt.asc") {
                    //    _1_soc_hlgt_comp model = new _1_soc_hlgt_comp();
                    //    model.MedraConfigId = MedraConfigId;
                    //    model.soc_code = Convert.ToInt64(allField[0]);
                    //    model.hlgt_code = Convert.ToInt64(allField[1]);
                    //    Context._1_soc_hlgt_comp.Add(model);
                    //}
                    //else if (fileName == fullPath + "\\mdhier.asc") {
                    //    _1_md_hierarchy model =new _1_md_hierarchy();
                    //    model.MedraConfigId = MedraConfigId;
                    //    model.pt_code = Convert.ToInt64(allField[0]);
                    //    model.hlt_code = Convert.ToInt64(allField[1]);
                    //    model.hlgt_code = Convert.ToInt64(allField[2]);
                    //    model.soc_code = Convert.ToInt64(allField[3]);
                    //    model.pt_name = allField[4];
                    //    model.hlt_name = allField[5];
                    //    model.hlgt_name = allField[6];
                    //    model.soc_name = allField[7];
                    //    model.soc_abbrev = allField[8];
                    //    model.null_field = allField[9];
                    //    model.pt_soc_code = allField[10] == "" ? (long?)null : Convert.ToInt64(allField[10]);
                    //    model.primary_soc_fg = allField[11];
                    //    Context._1_md_hierarchy.Add(model);
                    //}
                    //else if (fileName == fullPath + "\\intl_ord.asc") {
                    //    _1_soc_intl_order model = new _1_soc_intl_order();
                    //    model.MedraConfigId = MedraConfigId;
                    //    model.intl_ord_code = Convert.ToInt64(allField[0]);
                    //    model.soc_code = Convert.ToInt64(allField[1]);
                    //    Context._1_soc_intl_order.Add(model);
                    //}
                    //else if (fileName == fullPath + "\\smq_list.asc") {
                    //    _1_smq_list model = new _1_smq_list();
                    //    model.MedraConfigId = MedraConfigId;
                    //    model.smq_code = Convert.ToInt64(allField[0]);
                    //    model.smq_name = allField[1];
                    //    model.smq_level = Convert.ToInt32(allField[2]);
                    //    model.smq_description = allField[3];
                    //    model.smq_source = allField[4];
                    //    model.smq_note = allField[5];
                    //    model.MedDRA_version = allField[6];
                    //    model.status = allField[7];
                    //    model.smq_algorithm = allField[8];
                    //    Context._1_smq_list.Add(model);
                    //}
                    //else if (fileName == fullPath + "\\smq_content.asc") {
                    //    _1_smq_Content model = new _1_smq_Content();
                    //    model.MedraConfigId = MedraConfigId;
                    //    model.smq_code = Convert.ToInt64(allField[0]);
                    //    model.term_code = Convert.ToInt64(allField[1]);
                    //    model.term_level = Convert.ToInt32(allField[2]);
                    //    model.term_scope = Convert.ToInt32(allField[3]);
                    //    model.term_category = allField[4];
                    //    model.term_weight = Convert.ToInt32(allField[5]);
                    //    model.term_status = allField[6];
                    //    model.term_addition_version = allField[7];
                    //    model.term_last_modified_version = allField[8];
                    //    Context._1_smq_Content.Add(model);
                    //}
                }

                _uow.Save();
            }
            // ProcessFile(fileName);

        }

        public SummaryDto getSummary(int MedraConfigId)
        {
            SummaryDto count = new SummaryDto();
            count.Soc = Context.MeddraSocTerm.Where(x=>x.MedraConfigId == MedraConfigId).Count();
            count.Hlgt = Context.MeddraHlgtPrefTerm.Where(x => x.MedraConfigId == MedraConfigId).Count();
            count.Hlt = Context.MeddraHltPrefTerm.Where(x => x.MedraConfigId == MedraConfigId).Count();
            count.Pt = Context.MeddraPrefTerm.Where(x => x.MedraConfigId == MedraConfigId).Count();
            count.Llt = Context.MeddraLowLevelTerm.Where(x => x.MedraConfigId == MedraConfigId).Count();
            return count;
        }

        public void DeleteDirectory(string root)
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root,true);
            }
        }

    }
}
