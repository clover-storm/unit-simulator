using System.Collections.Generic;
using System.Linq;

namespace ReferenceModels.Validation;

/// <summary>
/// 검증 결과를 나타냅니다.
/// </summary>
public class ValidationResult
{
    /// <summary>검증 성공 여부 (오류가 없으면 true)</summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>오류 메시지 목록</summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>경고 메시지 목록</summary>
    public List<string> Warnings { get; init; } = new();

    /// <summary>
    /// 성공 결과를 생성합니다.
    /// </summary>
    public static ValidationResult Success() => new();

    /// <summary>
    /// 실패 결과를 생성합니다.
    /// </summary>
    public static ValidationResult Fail(params string[] errors)
    {
        return new ValidationResult
        {
            Errors = errors.ToList()
        };
    }

    /// <summary>
    /// 경고가 있는 성공 결과를 생성합니다.
    /// </summary>
    public static ValidationResult SuccessWithWarnings(params string[] warnings)
    {
        return new ValidationResult
        {
            Warnings = warnings.ToList()
        };
    }
}
