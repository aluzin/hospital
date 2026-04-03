using Hospital.Api.Contracts;
using Hospital.Api.Contracts.Patients;
using Hospital.Application.Patients.CreatePatient;
using Hospital.Application.Patients.DeletePatient;
using Hospital.Application.Patients.GetPatientById;
using Hospital.Application.Patients.UpdatePatient;
using Microsoft.AspNetCore.Mvc;
using ApiCreatePatientRequest = Hospital.Api.Contracts.Patients.CreatePatientRequest;
using ApiUpdatePatientRequest = Hospital.Api.Contracts.Patients.UpdatePatientRequest;

namespace Hospital.Api.Controllers;

[ApiController]
[Route("api/patients")]
[Tags("patients")]
public class PatientsController : ControllerBase
{
    private readonly ICreatePatientService _createPatientService;
    private readonly IGetPatientByIdService _getPatientByIdService;
    private readonly IUpdatePatientService _updatePatientService;
    private readonly IDeletePatientService _deletePatientService;

    public PatientsController(
        ICreatePatientService createPatientService,
        IGetPatientByIdService getPatientByIdService,
        IUpdatePatientService updatePatientService,
        IDeletePatientService deletePatientService)
    {
        _createPatientService = createPatientService;
        _getPatientByIdService = getPatientByIdService;
        _updatePatientService = updatePatientService;
        _deletePatientService = deletePatientService;
    }

    /// <summary>
    /// Creates a new patient.
    /// </summary>
    /// <remarks>
    /// Required fields are <c>name.family</c> and <c>birthDate</c>.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PatientResponse>> Create(
        [FromBody] ApiCreatePatientRequest request,
        CancellationToken cancellationToken)
    {
        var patient = await _createPatientService.ExecuteAsync(request.ToApplicationRequest(), cancellationToken);
        var response = patient.ToResponse();

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Returns a patient by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PatientResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var patient = await _getPatientByIdService.ExecuteAsync(new GetPatientByIdRequest
        {
            Id = id
        }, cancellationToken);

        return Ok(patient.ToResponse());
    }

    /// <summary>
    /// Updates an existing patient.
    /// </summary>
    /// <remarks>
    /// The full patient payload is expected in the request body.
    /// </remarks>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PatientResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PatientResponse>> Update(
        Guid id,
        [FromBody] ApiUpdatePatientRequest request,
        CancellationToken cancellationToken)
    {
        var patient = await _updatePatientService.ExecuteAsync(request.ToApplicationRequest(id), cancellationToken);

        return Ok(patient.ToResponse());
    }

    /// <summary>
    /// Deletes a patient by identifier.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _deletePatientService.ExecuteAsync(new DeletePatientRequest
        {
            Id = id
        }, cancellationToken);

        return NoContent();
    }
}
