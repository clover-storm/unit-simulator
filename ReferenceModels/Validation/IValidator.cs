namespace ReferenceModels.Validation;

/// <summary>
/// 레퍼런스 데이터의 유효성을 검증하는 인터페이스
/// </summary>
/// <typeparam name="T">검증 대상 레퍼런스 타입</typeparam>
public interface IValidator<T>
{
    /// <summary>
    /// 레퍼런스 데이터를 검증합니다.
    /// </summary>
    /// <param name="item">검증할 데이터</param>
    /// <param name="id">레퍼런스 ID</param>
    /// <returns>검증 결과</returns>
    ValidationResult Validate(T item, string id);
}
