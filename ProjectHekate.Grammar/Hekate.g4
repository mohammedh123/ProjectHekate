grammar Hekate;

/*
 * Parser Rules
 */

script
	:	functionDeclaration*
	;

functionDeclaration
	:	FUNCTION Identifier formalParameters functionBody
	;

functionBody
	:	block
	;

block
	:	LBRACE blockStatement* RBRACE
	;

localVariableDeclarationStatement
	:	localVariableDeclaration SEMI
	;

localVariableDeclaration
	:	nonConstVariableDeclaration
	|	constVariableDeclaration
	;

nonConstVariableDeclaration
	:	VAR Identifier ASSIGN expression
	;

constVariableDeclaration
	:	CONST nonConstVariableDeclaration
	;
	
formalParameter
	:	Identifier
	;

formalParameterList
	:	formalParameter (COMMA formalParameter)*
	;

formalParameters
	:	LPAREN formalParameterList? RPAREN
	;

// statements
statement
	:	block
	|	IF parExpression statement (ELSE statement)?
	|	FOR LPAREN forControl RPAREN statement
	|	WHILE parExpression statement
	|	BREAK SEMI
	|	CONTINUE SEMI
	|	SEMI
	|	statementExpression SEMI
	|	attachEmitterStatement SEMI
	|	fireStatement SEMI
	|	waitStatement SEMI
	;

attachEmitterStatement
	:	Identifier ATTACH Identifier parExpressionList withUpdaterOption?
	;

fireStatement
	:	FIRE Identifier parExpressionList fromEmitterOption
	;

waitStatement
	:	WAIT expression FRAMES
	;

fromEmitterOption
	:	FROM Identifier
	;

forControl
	:	forInit? SEMI expression? SEMI forUpdate
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
	:	LPAREN expression RPAREN
	;

expressionList
	:	expression (COMMA expression)*
	;

parExpressionList
	: LPAREN expressionList? RPAREN
	;

statementExpression
	:	expression
	;

constantExpression
	:	expression
	;

methodExpression
	:	Identifier parExpressionList
	;

expression
	:	primary
	|	expression (INC | DEC)
	|	(ADD|SUB|INC|DEC) expression
	|	BANG expression
	|   expression LPAREN expressionList? RPAREN
	|	expression (MUL|DIV|MOD) expression
	|	expression (ADD|SUB) expression
	|	expression (LE | GE | GT | LT) expression
	|	expression (EQUAL | NOTEQUAL) expression
	|	expression AND expression
	|	expression OR expression
	|	expression
		(	    ASSIGN
		|	ADD_ASSIGN
		|	SUB_ASSIGN
		|	MUL_ASSIGN
		|	DIV_ASSIGN
		)
		expression
	|	CREATE EMITTER parExpressionList withUpdaterOption?
	|	BUILD Identifier
	;

withUpdaterOption
	:	WITH UPDATER methodExpression
	;

primary
	:	LPAREN expression RPAREN
	|	literal
	|	Identifier
	|	ContextIdentifier
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
BREAK		: 'break';

CREATE		: 'create';
EMITTER		: 'emitter';
WITH		: 'with';
UPDATER		: 'updater';
ATTACH		: 'attach';
BUILD		: 'build';
FIRE		: 'fire';
FROM		: 'from';
WAIT		: 'wait';
FRAMES		: 'frames';

// Integer literals
IntegerLiteral
	:	'0'
	|	Sign? NonZeroDigit Digits*
	;

FloatingPointLiteral
	:	Sign? Digits '.' Digits?
	;

fragment
Digits
	:	Digit+
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
COMMA	: ',';

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
	|	ContextIdentifier
	;

ContextIdentifier
	: '$' Identifier
	;

fragment
Letter
	:	[a-zA-Z_]
	;

fragment
LetterOrDigit
	:	[a-zA-Z0-9_]
	;

WS 
	: [ \t\r\n\u000C]+ -> skip;

COMMENT
	: '/*' .*? '*/' -> skip;

LINE_COMMENT
	: '//' ~[\r\n]* -> skip;