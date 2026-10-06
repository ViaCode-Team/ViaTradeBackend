namespace ViaTrade.Application.Common.Exceptions;

public class BusinessRuleException(string message, string code = "business_rule_violation")
	: AppException(message, code) { }
