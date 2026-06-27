using ERP.Application.DTOs.Manufacturing;
using ERP.Domain.Enums.Manufacturing;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Web.Controllers;

public sealed partial class ManufacturingController
{
    [HttpPost("work-orders/{id:int}/release")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReleaseWorkOrder(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.ReleaseWorkOrderAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Work order released."
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to release work order." : result.ErrorMessage);

        return RedirectToAction(nameof(WorkOrders));
    }

    [HttpPost("work-orders/{id:int}/start")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartWorkOrder(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.StartWorkOrderAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Work order started."
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to start work order." : result.ErrorMessage);

        return RedirectToAction(nameof(WorkOrders));
    }

    [HttpPost("work-orders/{id:int}/complete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteWorkOrder(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.CompleteWorkOrderAsync(accessToken, id, null, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Work order completed."
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to complete work order." : result.ErrorMessage);

        return RedirectToAction(nameof(WorkOrders));
    }

    [HttpPost("work-orders/{id:int}/close")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseWorkOrder(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.CloseWorkOrderAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Work order closed."
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to close work order." : result.ErrorMessage);

        return RedirectToAction(nameof(WorkOrders));
    }

    [HttpPost("work-orders/{id:int}/cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelWorkOrder(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.CancelWorkOrderAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Work order cancelled."
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to cancel work order." : result.ErrorMessage);

        return RedirectToAction(nameof(WorkOrders));
    }

    [HttpPost("work-orders/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteWorkOrder(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.DeleteWorkOrderAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Work order deleted."
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to delete work order." : result.ErrorMessage);

        return RedirectToAction(nameof(WorkOrders));
    }

    [HttpPost("mrp/{id:int}/run")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RunMrp(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.RunMrpAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "MRP started."
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to run MRP." : result.ErrorMessage);

        return RedirectToAction(nameof(Mrp));
    }

    [HttpPost("mrp/{id:int}/complete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteMrp(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.CompleteMrpAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "MRP completed."
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to complete MRP." : result.ErrorMessage);

        return RedirectToAction(nameof(Mrp));
    }

    [HttpPost("mrp/{id:int}/cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelMrp(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.CancelMrpAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "MRP cancelled."
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to cancel MRP." : result.ErrorMessage);

        return RedirectToAction(nameof(Mrp));
    }

    [HttpPost("mrp/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMrp(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.DeleteMrpRunAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "MRP deleted."
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to delete MRP." : result.ErrorMessage);

        return RedirectToAction(nameof(Mrp));
    }

    [HttpPost("qc/{id:int}/start")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartQc(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.StartQcInspectionAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "QC inspection started."
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to start QC inspection." : result.ErrorMessage);

        return RedirectToAction(nameof(Qc));
    }

    [HttpPost("qc/{id:int}/pass")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PassQc(int id, CancellationToken ct = default)
    {
        return await CompleteQcWithResultAsync(id, QcResult.Pass, ct);
    }

    [HttpPost("qc/{id:int}/fail")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FailQc(int id, CancellationToken ct = default)
    {
        return await CompleteQcWithResultAsync(id, QcResult.Fail, ct);
    }

    [HttpPost("qc/{id:int}/cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelQc(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.CancelQcInspectionAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "QC inspection cancelled."
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to cancel QC inspection." : result.ErrorMessage);

        return RedirectToAction(nameof(Qc));
    }

    [HttpPost("qc/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQc(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.DeleteQcInspectionAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "QC inspection deleted."
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to delete QC inspection." : result.ErrorMessage);

        return RedirectToAction(nameof(Qc));
    }

    [HttpPost("rework/{id:int}/start")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> StartRework(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.StartReworkOrderAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Rework started."
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to start rework." : result.ErrorMessage);

        return RedirectToAction(nameof(Rework));
    }

    [HttpPost("rework/{id:int}/complete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteRework(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.CompleteReworkOrderAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Rework completed."
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to complete rework." : result.ErrorMessage);

        return RedirectToAction(nameof(Rework));
    }

    [HttpPost("rework/{id:int}/close")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloseRework(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.CloseReworkOrderAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Rework closed."
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to close rework." : result.ErrorMessage);

        return RedirectToAction(nameof(Rework));
    }

    [HttpPost("rework/{id:int}/cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelRework(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.CancelReworkOrderAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Rework cancelled."
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to cancel rework." : result.ErrorMessage);

        return RedirectToAction(nameof(Rework));
    }

    [HttpPost("rework/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRework(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.DeleteReworkOrderAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Rework deleted."
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to delete rework." : result.ErrorMessage);

        return RedirectToAction(nameof(Rework));
    }

    [HttpPost("scrap/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteScrap(int id, CancellationToken ct = default)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.DeleteScrapRecordAsync(accessToken, id, ct);
        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? "Scrap record deleted."
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to delete scrap record." : result.ErrorMessage);

        return RedirectToAction(nameof(Scrap));
    }

    private async Task<IActionResult> CompleteQcWithResultAsync(int id, QcResult resultType, CancellationToken ct)
    {
        var unauthorized = RequireAccessToken(out var accessToken, false);
        if (unauthorized is not null)
        {
            return unauthorized;
        }

        var result = await manufacturingApiClient.CompleteQcInspectionAsync(accessToken, id, new ManufacturingQcCompleteRequest
        {
            Result = resultType
        }, ct);

        var successText = resultType == QcResult.Pass ? "QC inspection marked pass." : "QC inspection marked fail.";

        TempData[result.IsSuccess ? "SuccessMessage" : "ErrorMessage"] = result.IsSuccess
            ? successText
            : (string.IsNullOrWhiteSpace(result.ErrorMessage) ? "Failed to complete QC inspection." : result.ErrorMessage);

        return RedirectToAction(nameof(Qc));
    }
}


