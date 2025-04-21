using GraduationProject.API.Requests;
using GraduationProject.API.Responses;

namespace GraduationProject.Application.Services.Interfaces
{
    public interface IGrammarCorrectionService
    {
        Task<ServiceResult<GrammarCorrectionResponse>> CorrectGrammar(GrammarCorrectionRequest request);
    }
}
