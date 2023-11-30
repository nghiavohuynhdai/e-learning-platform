using AutoMapper;
using Cursus.DTO;
using Cursus.DTO.Lesson;
using Cursus.Entities;
using Cursus.Services.Interfaces;
using Cursus.UnitOfWork;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Cursus.Services;

public class LessonService : ILessonService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISectionService _sectionService;

    public LessonService(IUnitOfWork unitOfWork, ISectionService sectionService)
    {
        _unitOfWork = unitOfWork;
        _sectionService = sectionService;
    }

    public async Task<Lesson> GetAsync(Guid id, Guid instructorId)
    {
        try
        {
            var lesson = await _unitOfWork.LessonRepository.GetByIdAsync(id);
            if (lesson is null || lesson.InstructorID != instructorId)
                return null;
            return lesson;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }

    public async Task<Lesson> CreateLesson(Lesson lesson)
    {
        try
        {
            var lessonNo = await _sectionService.CalculateNewSectionItemNo(lesson.SectionID);
            if (lessonNo == -1)
                throw new Exception("Fail to calculate new section item No");
            lesson.No = lessonNo;
            await _unitOfWork.LessonRepository.AddAsync(lesson);
            await _unitOfWork.CommitAsync();
            return lesson;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }

    public async Task<Lesson> UpdateLesson(Lesson lesson)
    {
        try
        {
            _unitOfWork.LessonRepository.Update(lesson);
            await _unitOfWork.CommitAsync();
            return lesson;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }

    public async Task<Lesson> DeleteLesson(Lesson lesson)
    {
        try
        {
            _unitOfWork.LessonRepository.Remove(lesson);
            var num = await _sectionService.UpdateSectionItemNoAfterDeleteItem(lesson.No, lesson.SectionID);
            if (num == 0) return null;
            await _unitOfWork.CommitAsync();
            return lesson;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return null;
        }
    }
}