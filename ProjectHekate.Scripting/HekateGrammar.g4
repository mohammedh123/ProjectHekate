grammar HekateGrammar;

/*
 * Parser Rules
 */

script
	:	functionDeclaration*
	;

functionDeclaration
	:	'function' Identifier formalParameters
		(
			functionBody
			|
			';'
		)
	;

functionBody
	:	block
	;

block
	:	'{' blockStatement* '}'
	;

localVariableDeclarationStatement
	:	localVariableDeclaration ';'
	;

localVariableDeclaration
	:	nonConstVariableDeclaration
	|	constVariableDeclaration
	;

nonConstVariableDeclaration
	:	'var' Identifier '=' expression
	;

constVariableDeclaration
	: 'const' nonConstVariableDeclaration
	;
	
formalParameter
	:	Identifier
	;

formalParameterList
	:	formalParameter (',' formalParameter)*
	;

formalParameters
	:	'(' formalParameterList? ')'
	;

// statements
statement
	:	block
	|	'if' parExpression statement ('else' statement)? ';'
	|	'for' '(' forControl ')' statement
	|	'while' parExpression statement
	|	'break;'
	|	'continue;'
	|	';'
	|	statementExpression ';'
	;

forControl
	:	forInit? ';' expression? ';' forUpdate
	;

forInit
	:	localVariableDeclaration
	|	expressionList
	;

forUpdate
	:	expressionList
	;
	
blockStatement
	:	localVariableDeclarationStatement
	|	statement
	; 

// expressions
parExpression
	:	'(' expression ')'
	;

expressionList
	:	expression (',' expression)*
	;

statementExpression
	:	expression
	;

constantExpression
	:	expression
	;

expression
	:	primary
	|	expression ('++' | '--')
	|	('+'|'-'|'++'|'--') expression
	|	'!' expression
	|	expression ('*'|'/'|'%') expression
	|	expression ('+'|'-') expression
	|	expression ('<=' | '>=' | '>' | '<') expression
	|	expression ('==' | '!=') expression
	|	expression '&&' expression
	|	expression '||' expression
	|	expression
		(	'='
		|	'+='
		|	'-='
		|	'*='
		|	'/='
		|	'%='
		)
		expression
	;

primary
	:	'(' expression ')'
	|	literal
	|	Identifier
	;

//literals
literal
	:	IntegerLiteral
	|	FloatingPointLiteral
	;

/*
 * Lexer Rules
 */

// Keywords
FUNCTION	: 'function';
CONST		: 'const';
VAR			: 'var';
FOR			: 'for';
WHILE		: 'while';
IF			: 'if';

CREATE_EMITTER	: 'create emitter';
WITH_UPDATER	: 'with updater';
ATTACH			: 'attach';
BUILD			: 'build';
FIRE			: 'fire';
FROM			: 'from';

// Integer literals
IntegerLiteral
	:	'0'
	|	Sign? NonZeroDigit (Digits?)
	;

FloatingPointLiteral
	:	Sign? Digits '.' Digits?
	;

fragment
Digits
	:	Digit (Digit)?
	;

fragment
Digit
	:	'0'
	|	NonZeroDigit
	;

fragment
NonZeroDigit
	:	[1-9]
	;
	
fragment
Sign
	:	[+-]
	;

// Separators
LPAREN	: '(';
RPAREN	: ')';
LBRACE	: '{';
RBRACE	: '}';
SEMI	: ';';

// Operators
ASSIGN		: '=';
GT			: '>';
LT			: '<';
BANG		: '!';
EQUAL		: '==';
LE			: '<=';
GE			: '>=';
NOTEQUAL	: '!=';
AND			: '&&';
OR			: '||';
INC			: '++';
DEC			: '--';
ADD			: '+';
SUB			: '-';
MUL			: '*';
DIV			: '/';
MOD         : '%';
ADD_ASSIGN	: '+=';
SUB_ASSIGN	: '-=';
MUL_ASSIGN	: '*=';
DIV_ASSIGN	: '/=';

// Identifiers
Identifier
	:	Letter LetterOrDigit*
	;

fragment
Letter
	:	[a-zA-Z]
	;

fragment
LetterOrDigit
	:	[a-zA-Z0-9]
	;

WS 
	: [ \t\r\n\u000C]+ -> skip;

COMMENT
	: '/*' .*? '*/' -> skip;

LINE_COMMENT
	: '//' ~[\r\n]* -> skip;