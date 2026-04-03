using Hospital.Application.Patients.CreatePatient;
using Hospital.Application.Patients.DeletePatient;
using Hospital.Application.Patients.GetPatientById;
using Hospital.Application.Patients.UpdatePatient;
using Microsoft.Extensions.DependencyInjection;

namespace Hospital.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICreatePatientService, CreatePatientService>();
        services.AddScoped<IGetPatientByIdService, GetPatientByIdService>();
        services.AddScoped<IUpdatePatientService, UpdatePatientService>();
        services.AddScoped<IDeletePatientService, DeletePatientService>();

        return services;
    }
}
