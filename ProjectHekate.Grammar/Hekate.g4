grammar Hekate;

/*
 * Parser Rules
 */

script
	:	(functionDeclaration|emitterUpdaterDeclaration|bulletUpdaterDeclaration)*
	;
	
variableDeclaration
	:	VAR Identifier ASSIGN expression
	;
	
functionDeclaration
	:	FUNCTION Identifier formalParameters functionBody
	;

emitterUpdaterDeclaration
	:	EUPDATER Identifier formalParameters updaterBody
	;

bulletUpdaterDeclaration
	:	BUPDATER Identifier formalParameters updaterBody
	;

functionBody
	:	block
	;

updaterBody
	:	block
	;

block
	:	LBRACE statement* RBRACE
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
	:	block	# BlockStatement
	|	IF parExpression statement (ELSE statement)?		# IfStatement
	|	FOR LPAREN forControl RPAREN statement				# ForStatement
	|	WHILE parExpression statement						# WhileStatement
	|	BREAK SEMI				# BreakStatement
	|	CONTINUE SEMI			# ContinueStatement
	|	SEMI					# EmptyStatement
	|	VAR Identifier ASSIGN createEmitterExpression SEMI	# CreateEmitterVariableStatement
	|	VAR Identifier ASSIGN buildEmitterExpression SEMI	# BuildEmitterVariableStatement
	|	variableDeclaration SEMI							# VariableDeclarationStatement
	|	expression SEMI			# ExpressionStatement
	|	Identifier ATTACH Identifier parExpressionList withUpdaterOption? SEMI		# AttachEmitterStatement
	|	FIRE Identifier parExpressionList fromEmitterOption SEMI	# FireStatement
	|	WAIT expression FRAMES SEMI		# WaitStatement
	|	RETURN expression SEMI			# ReturnStatement
	;
		
fromEmitterOption
	:	FROM Identifier
	;

forControl
	:	forInit? SEMI expression? SEMI forUpdate
	;

forInit
	:	variableDeclaration
	|	expressionList
	;

forUpdate
	:	expressionList
	;	

// expressions
parExpression
	:	LPAREN expression RPAREN
	;

expressionList
	:	expression (COMMA expression)*
	;

parExpressionList
	:	LPAREN expressionList? RPAREN
	;

updaterCallExpression
	:	Identifier parExpressionList
	;

createEmitterExpression
	:	CREATE EMITTER parExpressionList withUpdaterOption?
	;

buildEmitterExpression
	:	BUILD Identifier
	;

withUpdaterOption
	:	WITH UPDATER updaterCallExpression
	;

expression
	:	LPAREN expression RPAREN	# ParenthesizedExpression
	|	literal						# LiteralExpression
	|	Identifier					# IdentifierExpression
	|	ContextIdentifier			# ContextIdentifierExpression
	|	expression (INC|DEC)		# PostIncDecExpression
	|	(INC|DEC) expression		# PreIncDecExpression 
	|	SUB expression				# UnaryMinusExpression
	|	BANG expression				# NotExpression
	|	expression MUL expression	# MultiplyExpression
	|	expression DIV expression	# DivideExpression
	|	expression MOD expression	# ModulusExpression
	|	expression ADD expression	# AddExpression
	|	expression SUB expression	# SubExpression
	|	expression LE expression	# LessThanEqualExpression
	|	expression GE expression	# GreaterThanEqualExpression
	|	expression LT expression	# LessThanExpression
	|	expression GT expression	# GreaterThanExpression
	|	expression EQUAL expression		# EqualExpression
	|	expression NOTEQUAL expression	# NotEqualExpression
	|	expression AND expression	# AndExpression
	|	expression OR expression	# OrExpression
	|	Identifier ASSIGN expression	# AssignmentExpression
	|	Identifier ADD_ASSIGN expression	# AddAssignmentExpression
	|	Identifier SUB_ASSIGN expression	# SubAssignmentExpression
	|	Identifier MUL_ASSIGN expression	# MulAssignmentExpression
	|	Identifier DIV_ASSIGN expression	# DivAssignmentExpression
	|	Identifier parExpressionList		# FunctionCallExpression
	;
	
//literals
literal
	:	IntegerLiteral			# IntegerLiteral
	|	FloatingPointLiteral	# FloatingPointLiteral
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
RETURN		: 'return';

CREATE		: 'create';
EMITTER		: 'emitter';
WITH		: 'with';
UPDATER		: 'updater';
BUPDATER	: 'bulletUpdater';
EUPDATER	: 'emitterUpdater';
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
	:	[-]
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
ADD			: '+';
SUB			: '-';
MUL			: '*';
DIV			: '/';
MOD         : '%';
EQUAL		: '==';
LE			: '<=';
GE			: '>=';
NOTEQUAL	: '!=';
AND			: '&&';
OR			: '||';
INC			: '++';
DEC			: '--';
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
	: DOLLAR Identifier
	;

fragment
Letter
	:	[a-zA-Z_]
	;

fragment
LetterOrDigit
	:	[a-zA-Z0-9_]
	;

DOLLAR	: '$';

WS 
	: [ \t\r\n\u000C]+ -> skip;

COMMENT
	: '/*' .*? '*/' -> skip;

LINE_COMMENT
	: '//' ~[\r\n]* -> skip;