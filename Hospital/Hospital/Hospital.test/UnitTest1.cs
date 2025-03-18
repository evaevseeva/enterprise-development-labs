using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Polyclinic.Domain.Model; // ���������
using Polyclinic.Domain.Services; // ���������
using Polyclinic.Domain.Data; // ���������

namespace Polyclinic.Domain.Tests;

/// <summary>
/// ����� � ����-������� ��� �����������.
/// </summary>
public class PolyclinicTests
{
    private readonly DoctorInMemoryRepository _repository;

    public PolyclinicTests()
    {
        _repository = new DoctorInMemoryRepository();
    }

    /// <summary>
    /// ���� ������, ������������� ���������� � ���� ������, ���� ������ ������� �� ������ 10 ���.
    /// </summary>
    [Fact]
    public void GetDoctorsWithExperienceMoreThan10Years_ReturnsCorrectDoctors()
    {
        var result = _repository.GetDoctorsWithExperienceMoreThan10Years();

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var expectedDoctors = DataSeeder.Doctors
            .Where(d => d.Experience >= 10)
            .ToList();

        foreach (var doctor in expectedDoctors)
        {
            var expectedInfo = $"����: {doctor.FullName}, �������������: {doctor.Specialization}, ����: {doctor.Experience} ���";
            Assert.Contains(expectedInfo, result);
        }
    }

    /// <summary>
    /// ����������������� ���� ������, ������������� ������ ���������, ���������� �� ����� � ���������� �����, ������������� �� ���.
    /// </summary>
    /// <param name="doctorId">ID �����</param>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void GetPatientsByDoctor_ReturnsCorrectPatients(int doctorId)
    {
        var result = _repository.GetPatientsByDoctor(doctorId);

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var patients = DataSeeder.Appointments
            .Where(a => a.DoctorId == doctorId)
            .Select(a => a.Patient)
            .OrderBy(p => p.FullName)
            .ToList();

        foreach (var patient in patients)
        {
            var expectedInfo = $"�������: {patient.FullName}, �������: {patient.PassportNumber}, ��� ��������: {patient.BirthYear}";
            Assert.Contains(expectedInfo, result);
        }
    }

    /// <summary>
    /// ���� ������, ������������� ���������� � �������� �� ��������� ������ ���������.
    /// </summary>
    [Fact]
    public void GetHealthyPatients_ReturnsCorrectPatients()
    {
        var result = _repository.GetHealthyPatients();

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var healthyPatients = DataSeeder.Appointments
            .Where(a => a.Status == "������")
            .Select(a => a.Patient)
            .Distinct()
            .ToList();

        foreach (var patient in healthyPatients)
        {
            var expectedInfo = $"�������: {patient.FullName}, �������: {patient.PassportNumber}, ��� ��������: {patient.BirthYear}";
            Assert.Contains(expectedInfo, result);
        }
    }

    /// <summary>
    /// ���� ������, ������������� ���������� � ���������� ������� ��������� �� ������ �� ��������� �����.
    /// </summary>
    [Fact]
    public void GetAppointmentsCountByDoctorLastMonth_ReturnsCorrectCounts()
    {
        var result = _repository.GetAppointmentsCountByDoctorLastMonth();

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var lastMonth = DateTime.Now.AddMonths(-1);
        var appointmentsByDoctor = DataSeeder.Appointments
            .Where(a => a.AppointmentDateTime >= lastMonth)
            .GroupBy(a => a.DoctorId)
            .Select(g => new { DoctorId = g.Key, Count = g.Count() })
            .ToList();

        foreach (var appointment in appointmentsByDoctor)
        {
            var doctor = DataSeeder.Doctors.FirstOrDefault(d => d.Id == appointment.DoctorId);
            var expectedInfo = $"����: {doctor.FullName}, ���������� �������: {appointment.Count}";
            Assert.Contains(expectedInfo, result);
        }
    }

    /// <summary>
    /// ���� ������, ������������� ���������� � ��� 5 �������� ���������������� ������������ ����� ���������.
    /// </summary>
    [Fact]
    public void GetTop5Diseases_ReturnsTop5Diseases()
    {
        var result = _repository.GetTop5Diseases();

        Assert.NotNull(result);
        Assert.True(result.Count <= 5);

        var topDiseases = DataSeeder.Appointments
            .GroupBy(a => a.Conclusion)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => g.Key)
            .ToList();

        foreach (var disease in topDiseases)
        {
            Assert.Contains(disease, result);
        }
    }

    /// <summary>
    /// ���� ������, ������������� ���������� � ��������� ������ 30 ���, ������� �������� �� ����� � ���������� ������, ������������� �� ���� ��������.
    /// </summary>
    [Fact]
    public void GetPatientsOver30WithMultipleDoctors_ReturnsCorrectPatients()
    {
        var result = _repository.GetPatientsOver30WithMultipleDoctors();

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var currentYear = DateTime.Now.Year;
        var patientsOver30 = DataSeeder.Patients
            .Where(p => currentYear - p.BirthYear > 30)
            .Where(p => p.Appointments.Select(a => a.DoctorId).Distinct().Count() > 1)
            .OrderBy(p => p.BirthYear)
            .ToList();

        foreach (var patient in patientsOver30)
        {
            var expectedInfo = $"�������: {patient.FullName}, ��� ��������: {patient.BirthYear}, �����: {patient.Address}";
            Assert.Contains(expectedInfo, result);
        }
    }
}