using AutoMapper;
using DataAccess.Entities;
using Models;

namespace DataAccess
{
    public class ApplicationMapperProfile:Profile
    {
        public ApplicationMapperProfile()
        {
            // من User إلى UserDto
            CreateMap<User, UserDto>();

            // من User إلى UserListDto
            CreateMap<User, UserListDto>();

            // من CreateUserDto إلى User
            CreateMap<UserDto, User>()
                .ForMember(dest => dest.Password, opt => opt.Ignore()) // يتم تعيين كلمة المرور بشكل منفصل
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.Id, opt => opt.Ignore()); // الـ ID يتم توليده تلقائياً

            // من UpdateUserDto إلى User
            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<TeacherDto,Teacher>().ReverseMap();
            CreateMap<Teacher, TeacherDto>();

            CreateMap<CourseDto, Course>().ReverseMap();
            CreateMap<Course, CourseDto>();

            CreateMap<TeacherCourseDto, TeacherCourse>().ReverseMap();
            CreateMap<TeacherCourse, TeacherCourseDto>();

            CreateMap<BalanceDto, Balance>().ReverseMap();
            CreateMap<Balance, BalanceDto>();

            CreateMap<BalanceTransactionDto, BalanceTransaction>().ReverseMap();
            CreateMap<BalanceTransaction, BalanceTransactionDto>();
          
            CreateMap<CourseRegistrationDto, CourseRegistration>().ReverseMap();          
            CreateMap<CourseRegistration, CourseRegistrationDto>();
          
            CreateMap<TeacherGradeAssignmentDto, TeacherGradeAssignment>().ReverseMap();          
            CreateMap<TeacherGradeAssignment, TeacherGradeAssignmentDto>();
        }
    }
}
