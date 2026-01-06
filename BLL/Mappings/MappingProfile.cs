using AutoMapper;
using BLL.DTOs;
using BLL.DTOs.CourseDTOs;
using BLL.DTOs.EnrollmentDTOs;
using BLL.DTOs.LessonDTOs;
using BLL.DTOs.NotificationDTOs;
using BLL.DTOs.OptionDTOs;
using BLL.DTOs.ParentStudentDTOs;
using BLL.DTOs.QuestionDTOs;
using BLL.DTOs.QuizAttemptDTOs;
using BLL.DTOs.QuizDTOs;
using BLL.DTOs.SectionDTOs;
using BLL.DTOs.UserDTOs;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User Mappings
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(dest => dest.Roles, opt => opt.Ignore()); // Populated manually

            CreateMap<RegisterUserDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Course Mappings
            CreateMap<Course, CourseDto>()
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher.FullName))
                .ForMember(dest => dest.EnrollmentCount, opt => opt.Ignore());

            CreateMap<Course, CourseDetailDto>()
                .ForMember(dest => dest.TeacherName, opt => opt.MapFrom(src => src.Teacher.FullName))
                .ForMember(dest => dest.EnrollmentCount, opt => opt.Ignore())
                .ForMember(dest => dest.Sections, opt => opt.MapFrom(src => src.Sections))
                .ForMember(dest => dest.Quizzes, opt => opt.MapFrom(src => src.Quizzes));

            CreateMap<CreateCourseDto, Course>()
                 .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                 .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => false));
            CreateMap<UpdateCourseDto, Course>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CreateCourseDto, Course>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsPublished, opt => opt.MapFrom(src => false));

            CreateMap<UpdateCourseDto, Course>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CourseCode, CourseCodeDto>().ReverseMap();
            CreateMap<CreateCourseCodeDto, CourseCode>();
            CreateMap<UpdateCourseCodeDto, CourseCode>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // Section Mappings
            CreateMap<Section, SectionDto>();
            CreateMap<CreateSectionDto, Section>(); 

            // Lesson Mappings
            CreateMap<Lesson, LessonDto>()
                .ForMember(dest => dest.IsCompleted, opt => opt.Ignore());

            CreateMap<CreateLessonDto, Lesson>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Enrollment Mappings
            CreateMap<CourseEnrollment, EnrollmentDto>()
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.FullName))
                .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title))
                .ForMember(dest => dest.CompletionPercentage, opt => opt.Ignore());

            // Progress Mappings
            CreateMap<CourseProgress, CourseProgressDto>()
                .ForMember(dest => dest.CourseTitle, opt => opt.MapFrom(src => src.Course.Title))
                .ForMember(dest => dest.TotalLessons, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedLessons, opt => opt.Ignore());

            // Quiz Mappings
            CreateMap<Quiz, QuizDto>();

            CreateMap<Quiz, QuizDetailDto>()
                .ForMember(dest => dest.Questions, opt => opt.MapFrom(src => src.Questions));

            CreateMap<CreateQuizDto, Quiz>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Question Mappings
            CreateMap<Question, QuestionDto>()
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));

            CreateMap<CreateQuestionDto, Question>();

            // Option Mappings
            CreateMap<Option, OptionDto>();
            CreateMap<CreateOptionDto, Option>();

            // Quiz Attempt Mappings
            CreateMap<QuizAttempt, QuizAttemptDto>()
                .ForMember(dest => dest.QuizTitle, opt => opt.MapFrom(src => src.Quiz.Title))
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.FullName));

            // Notification Mappings
            CreateMap<Notification, NotificationDto>();
            CreateMap<CreateNotificationDto, Notification>()
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.IsRead, opt => opt.MapFrom(src => false));

            // Parent-Student Mappings
            CreateMap<ApplicationUser, StudentInfoDto>()
                .ForMember(dest => dest.LinkedAt, opt => opt.Ignore());

            // File Metadata Mappings
            CreateMap<FileMetadata, FileUploadDto>()
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.BunnyCdnUrl));
        }
    }
}
