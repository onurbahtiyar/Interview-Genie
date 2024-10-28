using Backend.Application.Interfaces;
using Backend.Common.Results;
using Backend.Domain.DTOs;
using Backend.Domain.Entities;
using Backend.Repository;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Backend.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Skill> _skillRepository;
        private readonly IRepository<Language> _languageRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<ProjectSkill> _projectSkillRepository;
        private readonly IMapper _mapper;

        public ProfileService(
            IRepository<User> userRepository,
            IRepository<Skill> skillRepository,
            IRepository<Language> languageRepository,
            IRepository<Project> projectRepository,
            IRepository<Company> companyRepository,
            IRepository<ProjectSkill> projectSkillRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _skillRepository = skillRepository;
            _languageRepository = languageRepository;
            _projectRepository = projectRepository;
            _companyRepository = companyRepository;
            _projectSkillRepository = projectSkillRepository;
            _mapper = mapper;
        }

        public async Task<IDataResult<ProfileDto>> GetProfileAsync(Guid userId)
        {
            try
            {
                // 1. Kullanıcıyı ve birinci seviye navigasyon özelliklerini al
                var user = await _userRepository.GetAsync(
                    u => u.Id == userId,
                    u => u.UserSkills,
                    u => u.UserLanguages,
                    u => u.Projects,
                    u => u.Companies
                );

                if (user == null)
                    return new ErrorDataResult<ProfileDto>(null, "Kullanıcı bulunamadı.");

                // 2. UserSkills'den SkillId'leri topla ve Skills'leri al
                var skillIds = user.UserSkills.Select(us => us.SkillId).ToList();
                var skills = await _skillRepository.GetAllAsync(s => skillIds.Contains(s.Id));

                foreach (var userSkill in user.UserSkills)
                {
                    userSkill.Skill = skills.FirstOrDefault(s => s.Id == userSkill.SkillId);
                }

                // 3. UserLanguages'den LanguageId'leri topla ve Languages'leri al
                var languageIds = user.UserLanguages.Select(ul => ul.LanguageId).ToList();
                var languages = await _languageRepository.GetAllAsync(l => languageIds.Contains(l.Id));

                foreach (var userLanguage in user.UserLanguages)
                {
                    userLanguage.Language = languages.FirstOrDefault(l => l.Id == userLanguage.LanguageId);
                }

                // 4. Projelerin ProjectSkills'lerini yükle
                foreach (var project in user.Projects)
                {
                    // Null kontrolü
                    if (project == null)
                    {
                        continue;
                    }

                    if (_projectSkillRepository == null)
                    {
                        throw new Exception("_projectSkillRepository is null. Please ensure it is properly initialized.");
                    }

                    if (project.Id == null || project.Id == Guid.Empty)
                    {
                        continue;
                    }


                    var projectSkills = await _projectSkillRepository.GetAllAsync(
                        ps => ps.ProjectId == project.Id
                    );

                    // Null kontrolü
                    if (projectSkills == null)
                    {
                        projectSkills = new List<ProjectSkill>();
                    }

                    project.ProjectSkills = projectSkills.ToList();
                }

                // 5. ProjectSkills'den SkillId'leri topla ve Skills'leri al
                var allProjectSkillIds = user.Projects
                    .Where(p => p.ProjectSkills != null)
                    .SelectMany(p => p.ProjectSkills)
                    .Select(ps => ps.SkillId)
                    .Distinct()
                    .ToList();

                var allSkills = await _skillRepository.GetAllAsync(s => allProjectSkillIds.Contains(s.Id));

                foreach (var project in user.Projects)
                {
                    if (project.ProjectSkills == null)
                        continue;

                    foreach (var projectSkill in project.ProjectSkills)
                    {
                        projectSkill.Skill = allSkills.FirstOrDefault(s => s.Id == projectSkill.SkillId);
                    }
                }

                // 6. Mapping işlemleri
                var skillDtos = user.UserSkills
                    .Where(us => us.Skill != null)
                    .Select(us => new SkillDto
                    {
                        Id = us.SkillId,
                        Name = us.Skill.Name
                    })
                    .ToList();

                var languageDtos = user.UserLanguages
                    .Where(ul => ul.Language != null)
                    .Select(ul => new LanguageDto
                    {
                        Id = ul.LanguageId,
                        Name = ul.Language.Name
                    })
                    .ToList();

                var projectDtos = user.Projects.Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    Skills = p.ProjectSkills != null
                        ? p.ProjectSkills
                            .Where(ps => ps.Skill != null)
                            .Select(ps => new SkillDto
                            {
                                Id = ps.SkillId,
                                Name = ps.Skill.Name
                            })
                            .ToList()
                        : new List<SkillDto>(),
                }).ToList();

                var companyDtos = _mapper.Map<List<CompanyDto>>(user.Companies);

                // ProfileDto'yu oluştur
                var profileDto = new ProfileDto
                {
                    UserId = user.Id,
                    Skills = skillDtos,
                    Languages = languageDtos,
                    Projects = projectDtos,
                    Companies = companyDtos
                };

                return new SuccessDataResult<ProfileDto>(profileDto, "Profil başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<ProfileDto>(null, $"Bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> UpdateSkillsAsync(Guid userId, List<string> skills)
        {
            try
            {
                var user = await _userRepository.GetAsync(u => u.Id == userId, u => u.UserSkills);
                if (user == null)
                    return new ErrorResult("Kullanıcı bulunamadı.");

                // Mevcut yetenekleri temizle
                user.UserSkills.Clear();

                // Yeni yetenekleri ekle veya mevcutları kullan
                foreach (var skillName in skills)
                {
                    var skill = await _skillRepository.GetAsync(s => s.Name == skillName);
                    if (skill == null)
                    {
                        skill = new Skill { Name = skillName };
                        await _skillRepository.AddAsync(skill);
                    }

                    user.UserSkills.Add(new UserSkill { UserId = userId, SkillId = skill.Id });
                }

                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();

                return new SuccessResult("Yetenekler başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> UpdateLanguagesAsync(Guid userId, List<string> languages)
        {
            try
            {
                var user = await _userRepository.GetAsync(u => u.Id == userId, u => u.UserLanguages);
                if (user == null)
                    return new ErrorResult("Kullanıcı bulunamadı.");

                // Mevcut dilleri temizle
                user.UserLanguages.Clear();

                // Yeni dilleri ekle veya mevcutları kullan
                foreach (var languageName in languages)
                {
                    var language = await _languageRepository.GetAsync(l => l.Name == languageName);
                    if (language == null)
                    {
                        language = new Language { Name = languageName };
                        await _languageRepository.AddAsync(language);
                    }

                    user.UserLanguages.Add(new UserLanguage { UserId = userId, LanguageId = language.Id });
                }

                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();

                return new SuccessResult("Diller başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> AddOrUpdateProjectsAsync(Guid userId, List<ProjectDto> projects)
        {
            try
            {
                var user = await _userRepository.GetAsync(u => u.Id == userId, u => u.Projects);
                if (user == null)
                    return new ErrorResult("Kullanıcı bulunamadı.");

                if (projects == null || projects.Count == 0)
                {
                    _projectRepository.RemoveRange(user.Projects);
                    await _projectRepository.SaveChangesAsync();
                }

                foreach (var projectDto in projects)
                {
                    var existingProject = user.Projects.FirstOrDefault(p => p.Id == projectDto.Id);

                    if (existingProject != null)
                    {
                        // Güncelle
                        existingProject.Title = projectDto.Title;
                        existingProject.Description = projectDto.Description;

                        // Projeye ait mevcut yetenekleri temizle
                        foreach (var item in await _projectSkillRepository.GetAllAsync(x=> x.ProjectId == projectDto.Id))
                        {
                            _projectSkillRepository.Delete(item);
                        }

                        // Yeni yetenekleri ekle veya mevcutları kullan
                        foreach (var skillDto in projectDto.Skills)
                        {
                            var skill = await _skillRepository.GetAsync(s => s.Name == skillDto.Name);
                            if (skill == null)
                            {
                                skill = new Skill { Name = skillDto.Name };
                                await _skillRepository.AddAsync(skill);
                            }

                            existingProject.ProjectSkills.Add(new ProjectSkill
                            {
                                ProjectId = existingProject.Id,
                                SkillId = skill.Id
                            });
                        }

                        _projectRepository.Update(existingProject);
                    }
                    else
                    {
                        // Ekle
                        var newProject = new Project
                        {
                            Id = Guid.NewGuid(),
                            Title = projectDto.Title,
                            Description = projectDto.Description,
                            UserId = userId,
                            ProjectSkills = new List<ProjectSkill>()
                        };

                        foreach (var skillDto in projectDto.Skills)
                        {
                            var skill = await _skillRepository.GetAsync(s => s.Name == skillDto.Name);
                            if (skill == null)
                            {
                                skill = new Skill { Name = skillDto.Name };
                                await _skillRepository.AddAsync(skill);
                            }

                            newProject.ProjectSkills.Add(new ProjectSkill
                            {
                                ProjectId = newProject.Id,
                                SkillId = skill.Id
                            });
                        }

                        await _projectRepository.AddAsync(newProject);
                        user.Projects.Add(newProject);
                    }
                }

                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();

                return new SuccessResult("Projeler başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> AddOrUpdateCompaniesAsync(Guid userId, List<CompanyDto> companies)
        {
            try
            {
                var user = await _userRepository.GetAsync(u => u.Id == userId, u => u.Companies);
                if (user == null)
                    return new ErrorResult("Kullanıcı bulunamadı.");

                // Boş gönderildiyse sil
                if (companies == null || companies.Count == 0)
                {
                    _companyRepository.RemoveRange(user.Companies);
                    await _companyRepository.SaveChangesAsync();
                }

                // Mevcut şirketleri güncelle veya yeni şirketleri ekle
                foreach (var companyDto in companies)
                {
                    var existingCompany = user.Companies.FirstOrDefault(c => c.Id == companyDto.Id);
                    if (existingCompany != null)
                    {
                        // Güncelle
                        existingCompany.CompanyName = companyDto.CompanyName;
                        existingCompany.Position = companyDto.Position;
                        existingCompany.Description = companyDto.Description;

                        _companyRepository.Update(existingCompany);
                    }
                    else
                    {
                        // Ekle
                        var newCompany = _mapper.Map<Company>(companyDto);
                        newCompany.UserId = userId;
                        await _companyRepository.AddAsync(newCompany);
                        user.Companies.Add(newCompany);
                    }
                }

                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();

                return new SuccessResult("Şirketler başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IDataResult<List<SkillDto>>> GetAllSkillsAsync()
        {
            try
            {
                var skills = await _skillRepository.GetAllAsync();
                var skillDtos = skills.Select(s => new SkillDto
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList();

                return new SuccessDataResult<List<SkillDto>>(skillDtos, "Yetenekler başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<SkillDto>>(null, $"Bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IDataResult<List<LanguageDto>>> GetAllLanguagesAsync()
        {
            try
            {
                var languages = await _languageRepository.GetAllAsync();
                var languageDtos = languages.Select(l => new LanguageDto
                {
                    Id = l.Id,
                    Name = l.Name
                }).ToList();

                return new SuccessDataResult<List<LanguageDto>>(languageDtos, "Diller başarıyla getirildi.");
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<LanguageDto>>(null, $"Bir hata oluştu: {ex.Message}");
            }
        }
    }
}
