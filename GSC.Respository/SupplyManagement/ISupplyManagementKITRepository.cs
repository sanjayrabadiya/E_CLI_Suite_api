﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Helper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace GSC.Respository.SupplyManagement
{
    public interface ISupplyManagementKITRepository : IGenericRepository<SupplyManagementKIT>
    {
        List<SupplyManagementKITGridDto> GetKITList(bool isDeleted, int ProjectId);

        IList<DropDownDto> GetVisitDropDownByAllocation(int projectId);

        List<KitListApproved> getApprovedKit(int id);

        string GenerateKitNo(SupplyManagementKitNumberSettings kitsettings, int noseriese);

        int GetAvailableRemainingkitCount(int ProjectId, int PharmacyStudyProductTypeId);

        void InsertKitRandomizationDetail(SupplyManagementVisitKITDetailDto supplyManagementVisitKITDetailDto);

        List<SupplyManagementVisitKITDetailGridDto> GetRandomizationKitNumberAssignList(int projectId, int siteId, int id);

        List<DropDownDto> GetRandomizationDropdownKit(int projectid);

        SupplyManagementVisitKITDetailDto SetKitNumber(SupplyManagementVisitKITDetailDto obj);

        void InsertKitHistory(SupplyManagementKITDetailHistory supplyManagementKITDetailHistory);

        List<SupplyManagementKITDetailHistoryDto> KitHistoryList(int id);

        List<SupplyManagementKITReturnGridDto> GetKitReturnList(int projectId, KitStatusRandomization kitType, int? siteId, int? visitId, int? randomizationId);

        SupplyManagementKITReturnGridDto ReturnSave(SupplyManagementKITReturnGridDto obj);

        void ReturnSaveAll(SupplyManagementKITReturnDtofinal data);

        List<SupplyManagementKITDiscardGridDto> GetKitDiscardList(int projectId, KitStatusRandomization kitType, int? siteId, int? visitId, int? randomizationId);

        void KitDiscard(SupplyManagementKITDiscardDtofinal data);

        void KitSendtoSponser(SupplyManagementKITDiscardDtofinal data);

        void SendKitReturnEmail(SupplyManagementKITReturnDtofinal obj);

        void returnVerificationStatus(SupplyManagementKITReturnVerificationDto data);

        List<SupplyManagementKITSeriesGridDto> GetKITSeriesList(bool isDeleted, int ProjectId);

        List<SupplyManagementKITSeriesDetailGridDto> GetKITSeriesDetailList(int id);

        List<SupplyManagementKITSeriesDetailHistoryGridDto> GetKITSeriesDetailHistoryList(int id);

        string CheckAvailableQtySequenceKit(SupplyManagementKITSeriesDto supplyManagementKITSeriesDto);

        void InsertKitSequenceRandomizationDetail(SupplyManagementVisitKITSequenceDetailDto supplyManagementVisitKITDetailDto);

        void InsertKitSequenceHistory(SupplyManagementKITSeriesDetailHistory supplyManagementVisitKITDetailHistory);

        void returnVerificationStatusSequence(SupplyManagementKITReturnVerificationSequenceDto data);
    }
}